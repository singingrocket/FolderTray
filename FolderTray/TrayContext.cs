using System.Diagnostics;
using System.Runtime.InteropServices;

class TrayContext : ApplicationContext
{
    NotifyIcon _ni = new() { Visible = true, Text = "Folder Tray" };
    ContextMenuStrip _menu = new();
    List<string> _roots = new();
    string _cfg = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "FolderTray", "roots.txt");

    // P/Invoke for extracting shell icons
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
        public uint dwAttributes;
    }

    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_SMALLICON = 0x1;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
    private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    public TrayContext()
    {
        // Load custom icon from Icons folder
        string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "folder.ico");
        if (File.Exists(iconPath))
            _ni.Icon = new Icon(iconPath);
        else
            _ni.Icon = SystemIcons.Application; // Fallback to system icon

        _ni.ContextMenuStrip = _menu;
        _ni.MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) _menu.Show(Cursor.Position); };
        RebuildMenu();
    }
    void RebuildMenu()
    {
        _menu.Items.Clear(); LoadRoots();
        _menu.Items.Add("Add Folder...", null, (s, e) => AddRoot());
        _menu.Items.Add("Refresh", null, (s, e) => RebuildMenu());
        _menu.Items.Add("Exit", null, (s, e) => { _ni.Visible = false; Application.Exit(); });
        _menu.Items.Add(new ToolStripSeparator());
        foreach (var r in _roots.Where(Directory.Exists))
            AddFolderNode(_menu.Items, new DirectoryInfo(r), isRoot: true);
    }
    void AddRoot()
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _roots.Add(dlg.SelectedPath); SaveRoots(); RebuildMenu();
        }
    }
    void LoadRoots()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_cfg)!);
        _roots = File.Exists(_cfg) ? File.ReadAllLines(_cfg).ToList() : new();
    }
    void SaveRoots() => File.WriteAllLines(_cfg, _roots);

    Image? GetFolderIcon()
    {
        try
        {
            // Use shell32.dll icon for folders
            string shell32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
            Icon? icon = Icon.ExtractAssociatedIcon(shell32);
            if (icon != null)
            {
                // Get the standard folder icon from shell32
                SHFILEINFO shfi = new SHFILEINFO();
                IntPtr result = SHGetFileInfo("dummy", FILE_ATTRIBUTE_DIRECTORY, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);
                if (result != IntPtr.Zero && shfi.hIcon != IntPtr.Zero)
                {
                    Icon folderIcon = Icon.FromHandle(shfi.hIcon);
                    Bitmap bitmap = new Bitmap(folderIcon.ToBitmap(), 16, 16);
                    DestroyIcon(shfi.hIcon);
                    return bitmap;
                }
            }
        }
        catch { }
        return null;
    }

    Image? GetFileIcon(string filePath)
    {
        try
        {
            // Try to get the associated icon for the file
            Icon? icon = Icon.ExtractAssociatedIcon(filePath);
            if (icon != null)
            {
                Bitmap bitmap = new Bitmap(icon.ToBitmap(), 16, 16);
                return bitmap;
            }
        }
        catch
        {
            // If that fails, try using SHGetFileInfo
            try
            {
                SHFILEINFO shfi = new SHFILEINFO();
                string ext = Path.GetExtension(filePath);
                IntPtr result = SHGetFileInfo(ext, FILE_ATTRIBUTE_NORMAL, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);
                if (result != IntPtr.Zero && shfi.hIcon != IntPtr.Zero)
                {
                    Icon fileIcon = Icon.FromHandle(shfi.hIcon);
                    Bitmap bitmap = new Bitmap(fileIcon.ToBitmap(), 16, 16);
                    DestroyIcon(shfi.hIcon);
                    return bitmap;
                }
            }
            catch { }
        }
        return null;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    void AddFolderNode(ToolStripItemCollection items, DirectoryInfo dir, bool isRoot = false)
    {
        var m = new ToolStripMenuItem(dir.Name);
        m.Image = GetFolderIcon();
        m.DropDownOpening += (s, e) => {
            m.DropDownItems.Clear();
            m.DropDownItems.Add("Open in Explorer", null, (s2, e2) => Process.Start("explorer.exe", dir.FullName));

            // Add "Remove Folder" option only for root folders
            if (isRoot)
            {
                m.DropDownItems.Add("Remove Folder", null, (s2, e2) => RemoveRoot(dir.FullName));
            }

            m.DropDownItems.Add(new ToolStripSeparator());
            foreach (var d in dir.GetDirectories()) AddFolderNode(m.DropDownItems, d, isRoot: false);
            foreach (var f in dir.GetFiles())
            {
                var fileItem = new ToolStripMenuItem(f.Name, GetFileIcon(f.FullName), (s3, e3) => Process.Start(new ProcessStartInfo(f.FullName) { UseShellExecute = true }));
                m.DropDownItems.Add(fileItem);
            }
        };
        items.Add(m);
    }

    void RemoveRoot(string path)
    {
        if (_roots.Contains(path))
        {
            _roots.Remove(path);
            SaveRoots();
            RebuildMenu();
        }
    }

}
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class WindowsAPI : MonoBehaviour
{
    public static bool WinAPI_EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags)
    {
        return EnumDisplayDevices(lpDevice, iDevNum, ref lpDisplayDevice, dwFlags);
    }

    public static bool WinAPI_EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode)
    {
        return EnumDisplaySettings(deviceName, modeNum, ref devMode);
    }

    public static IntPtr WinAPI_GetActiveWindow()
    {
        return GetActiveWindow();
    }

    public static IntPtr WinAPI_GetCurrentProcess()
    {
        return GetCurrentProcess();
    }

    public static IntPtr WinAPI_GetDC(IntPtr hWnd)
    {
        return GetDC(hWnd);
    }

    public static int WinAPI_GetDeviceCaps(IntPtr hdc, int nIndex)
    {
        return GetDeviceCaps(hdc, nIndex);
    }

    public static IntPtr WinAPI_GetForegroundWindow()
    {
        return GetForegroundWindow();
    }

    public static bool WinAPI_GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi)
    {
        return GetMonitorInfo(hMonitor, ref lpmi);
    }

    public static void WinAPI_GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS awareness)
    {
        GetProcessDpiAwareness(hprocess, out awareness);
    }

    public static int WinAPI_GetSystemMetrics(SystemMetric smIndex)
    {
        return GetSystemMetrics(smIndex);
    }

    public static IntPtr WinAPI_MonitorFromPoint(POINT pt, MonitorOptions dwFlags)
    {
        return MonitorFromPoint(pt, dwFlags);
    }

    public static bool WinAPI_ReleaseDC(IntPtr hWnd, IntPtr hDC)
    {
        return ReleaseDC(hWnd, hDC);
    }

    public static bool WinAPI_SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness)
    {
        return SetProcessDpiAwareness(awareness);
    }

    public static bool WinAPI_SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags)
    {
        return SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
    }

    public static bool WinAPI_SetWindowText(IntPtr hwnd, String lpString)
    {
        return SetWindowText(hwnd, lpString);
    }

    public static bool WinAPI_ShowWindow(IntPtr hWnd, int nCmdShow)
    {
        return ShowWindow(hWnd, nCmdShow);
    }

    public static IntPtr WinAPI_GetWindowLongPtr(IntPtr hWnd, WindowLongFlags nIndex)
    {
        return GetWindowLongPtr(hWnd, (int)nIndex);
    }

    public static IntPtr WinAPI_SetWindowLongPtr(IntPtr hWnd, WindowLongFlags nIndex, IntPtr dwNewLong)
    {
        return SetWindowLongPtr(hWnd, (int)nIndex, dwNewLong);
    }

    public static uint WinAPI_GetCurrentThreadId()
    {
        return GetCurrentThreadId();
    }

    public static uint WinAPI_GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId)
    {
        return GetWindowThreadProcessId(hWnd, out lpdwProcessId);
    }

    public static uint WinAPI_GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId)
    {
        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        return GetWindowThreadProcessId(hWnd, ProcessId);
    }

    public static bool WinAPI_AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach)
    {
        return AttachThreadInput(idAttach, idAttachTo, fAttach);
    }

    public static IntPtr WinAPI_SetFocus(IntPtr hWnd)
    {
        return SetFocus(hWnd);
    }

    public static IntPtr WinAPI_SetActiveWindow(IntPtr hWnd)
    {
        return SetActiveWindow(hWnd);
    }

    public static bool WinAPI_SetForegroundWindow(IntPtr hWnd)
    {
        return SetForegroundWindow(hWnd);
    }

    public static int WinAPI_GetClassName(IntPtr hWnd, out string lpString)
    {
        StringBuilder sb = new StringBuilder(256);
        int ret = GetClassName(hWnd, sb, sb.Capacity);
        lpString = sb.ToString();
        return ret;
    }

    public static bool WinAPI_EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam)
    {
        return EnumThreadWindows(dwThreadId, lpEnumFunc, lParam);
    }



    //////////////////////////////////////////////////////
    // Most of this code below taken from pinvoke.net
    //////////////////////////////////////////////////////


    public const int SW_MAXIMIZE = 3;  // for ShowWindow()

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// Flags used with the Windows API (User32.dll):GetSystemMetrics(SystemMetric smIndex)
    ///
    /// This Enum and declaration signature was written by Gabriel T. Sharp
    /// ai_productions@verizon.net or osirisgothra@hotmail.com
    /// Obtained on pinvoke.net, please contribute your code to support the wiki!
    /// </summary>
    public enum SystemMetric : int
    {
        /// <summary>
        /// The flags that specify how the system arranged minimized windows. For more information, see the Remarks section in this topic.
        /// </summary>
        SM_ARRANGE = 56,

        /// <summary>
        /// The value that specifies how the system is started:
        /// 0 Normal boot
        /// 1 Fail-safe boot
        /// 2 Fail-safe with network boot
        /// A fail-safe boot (also called SafeBoot, Safe Mode, or Clean Boot) bypasses the user startup files.
        /// </summary>
        SM_CLEANBOOT = 67,

        /// <summary>
        /// The number of display monitors on a desktop. For more information, see the Remarks section in this topic.
        /// </summary>
        SM_CMONITORS = 80,

        /// <summary>
        /// The number of buttons on a mouse, or zero if no mouse is installed.
        /// </summary>
        SM_CMOUSEBUTTONS = 43,

        /// <summary>
        /// The width of a window border, in pixels. This is equivalent to the SM_CXEDGE value for windows with the 3-D look.
        /// </summary>
        SM_CXBORDER = 5,

        /// <summary>
        /// The width of a cursor, in pixels. The system cannot create cursors of other sizes.
        /// </summary>
        SM_CXCURSOR = 13,

        /// <summary>
        /// This value is the same as SM_CXFIXEDFRAME.
        /// </summary>
        SM_CXDLGFRAME = 7,

        /// <summary>
        /// The width of the rectangle around the location of a first click in a double-click sequence, in pixels. ,
        /// The second click must occur within the rectangle that is defined by SM_CXDOUBLECLK and SM_CYDOUBLECLK for the system
        /// to consider the two clicks a double-click. The two clicks must also occur within a specified time.
        /// To set the width of the double-click rectangle, call SystemParametersInfo with SPI_SETDOUBLECLKWIDTH.
        /// </summary>
        SM_CXDOUBLECLK = 36,

        /// <summary>
        /// The number of pixels on either side of a mouse-down point that the mouse pointer can move before a drag operation begins.
        /// This allows the user to click and release the mouse button easily without unintentionally starting a drag operation.
        /// If this value is negative, it is subtracted from the left of the mouse-down point and added to the right of it.
        /// </summary>
        SM_CXDRAG = 68,

        /// <summary>
        /// The width of a 3-D border, in pixels. This metric is the 3-D counterpart of SM_CXBORDER.
        /// </summary>
        SM_CXEDGE = 45,

        /// <summary>
        /// The thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels.
        /// SM_CXFIXEDFRAME is the height of the horizontal border, and SM_CYFIXEDFRAME is the width of the vertical border.
        /// This value is the same as SM_CXDLGFRAME.
        /// </summary>
        SM_CXFIXEDFRAME = 7,

        /// <summary>
        /// The width of the left and right edges of the focus rectangle that the DrawFocusRectdraws.
        /// This value is in pixels.
        /// Windows 2000:  This value is not supported.
        /// </summary>
        SM_CXFOCUSBORDER = 83,

        /// <summary>
        /// This value is the same as SM_CXSIZEFRAME.
        /// </summary>
        SM_CXFRAME = 32,

        /// <summary>
        /// The width of the client area for a full-screen window on the primary display monitor, in pixels.
        /// To get the coordinates of the portion of the screen that is not obscured by the system taskbar or by application desktop toolbars,
        /// call the SystemParametersInfofunction with the SPI_GETWORKAREA value.
        /// </summary>
        SM_CXFULLSCREEN = 16,

        /// <summary>
        /// The width of the arrow bitmap on a horizontal scroll bar, in pixels.
        /// </summary>
        SM_CXHSCROLL = 21,

        /// <summary>
        /// The width of the thumb box in a horizontal scroll bar, in pixels.
        /// </summary>
        SM_CXHTHUMB = 10,

        /// <summary>
        /// The default width of an icon, in pixels. The LoadIcon function can load only icons with the dimensions
        /// that SM_CXICON and SM_CYICON specifies.
        /// </summary>
        SM_CXICON = 11,

        /// <summary>
        /// The width of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size
        /// SM_CXICONSPACING by SM_CYICONSPACING when arranged. This value is always greater than or equal to SM_CXICON.
        /// </summary>
        SM_CXICONSPACING = 38,

        /// <summary>
        /// The default width, in pixels, of a maximized top-level window on the primary display monitor.
        /// </summary>
        SM_CXMAXIMIZED = 61,

        /// <summary>
        /// The default maximum width of a window that has a caption and sizing borders, in pixels.
        /// This metric refers to the entire desktop. The user cannot drag the window frame to a size larger than these dimensions.
        /// A window can override this value by processing the WM_GETMINMAXINFO message.
        /// </summary>
        SM_CXMAXTRACK = 59,

        /// <summary>
        /// The width of the default menu check-mark bitmap, in pixels.
        /// </summary>
        SM_CXMENUCHECK = 71,

        /// <summary>
        /// The width of menu bar buttons, such as the child window close button that is used in the multiple document interface, in pixels.
        /// </summary>
        SM_CXMENUSIZE = 54,

        /// <summary>
        /// The minimum width of a window, in pixels.
        /// </summary>
        SM_CXMIN = 28,

        /// <summary>
        /// The width of a minimized window, in pixels.
        /// </summary>
        SM_CXMINIMIZED = 57,

        /// <summary>
        /// The width of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged.
        /// This value is always greater than or equal to SM_CXMINIMIZED.
        /// </summary>
        SM_CXMINSPACING = 47,

        /// <summary>
        /// The minimum tracking width of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions.
        /// A window can override this value by processing the WM_GETMINMAXINFO message.
        /// </summary>
        SM_CXMINTRACK = 34,

        /// <summary>
        /// The amount of border padding for captioned windows, in pixels. Windows XP/2000:  This value is not supported.
        /// </summary>
        SM_CXPADDEDBORDER = 92,

        /// <summary>
        /// The width of the screen of the primary display monitor, in pixels. This is the same value obtained by calling
        /// GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, HORZRES).
        /// </summary>
        SM_CXSCREEN = 0,

        /// <summary>
        /// The width of a button in a window caption or title bar, in pixels.
        /// </summary>
        SM_CXSIZE = 30,

        /// <summary>
        /// The thickness of the sizing border around the perimeter of a window that can be resized, in pixels.
        /// SM_CXSIZEFRAME is the width of the horizontal border, and SM_CYSIZEFRAME is the height of the vertical border.
        /// This value is the same as SM_CXFRAME.
        /// </summary>
        SM_CXSIZEFRAME = 32,

        /// <summary>
        /// The recommended width of a small icon, in pixels. Small icons typically appear in window captions and in small icon view.
        /// </summary>
        SM_CXSMICON = 49,

        /// <summary>
        /// The width of small caption buttons, in pixels.
        /// </summary>
        SM_CXSMSIZE = 52,

        /// <summary>
        /// The width of the virtual screen, in pixels. The virtual screen is the bounding rectangle of all display monitors.
        /// The SM_XVIRTUALSCREEN metric is the coordinates for the left side of the virtual screen.
        /// </summary>
        SM_CXVIRTUALSCREEN = 78,

        /// <summary>
        /// The width of a vertical scroll bar, in pixels.
        /// </summary>
        SM_CXVSCROLL = 2,

        /// <summary>
        /// The height of a window border, in pixels. This is equivalent to the SM_CYEDGE value for windows with the 3-D look.
        /// </summary>
        SM_CYBORDER = 6,

        /// <summary>
        /// The height of a caption area, in pixels.
        /// </summary>
        SM_CYCAPTION = 4,

        /// <summary>
        /// The height of a cursor, in pixels. The system cannot create cursors of other sizes.
        /// </summary>
        SM_CYCURSOR = 14,

        /// <summary>
        /// This value is the same as SM_CYFIXEDFRAME.
        /// </summary>
        SM_CYDLGFRAME = 8,

        /// <summary>
        /// The height of the rectangle around the location of a first click in a double-click sequence, in pixels.
        /// The second click must occur within the rectangle defined by SM_CXDOUBLECLK and SM_CYDOUBLECLK for the system to consider
        /// the two clicks a double-click. The two clicks must also occur within a specified time. To set the height of the double-click
        /// rectangle, call SystemParametersInfo with SPI_SETDOUBLECLKHEIGHT.
        /// </summary>
        SM_CYDOUBLECLK = 37,

        /// <summary>
        /// The number of pixels above and below a mouse-down point that the mouse pointer can move before a drag operation begins.
        /// This allows the user to click and release the mouse button easily without unintentionally starting a drag operation.
        /// If this value is negative, it is subtracted from above the mouse-down point and added below it.
        /// </summary>
        SM_CYDRAG = 69,

        /// <summary>
        /// The height of a 3-D border, in pixels. This is the 3-D counterpart of SM_CYBORDER.
        /// </summary>
        SM_CYEDGE = 46,

        /// <summary>
        /// The thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels.
        /// SM_CXFIXEDFRAME is the height of the horizontal border, and SM_CYFIXEDFRAME is the width of the vertical border.
        /// This value is the same as SM_CYDLGFRAME.
        /// </summary>
        SM_CYFIXEDFRAME = 8,

        /// <summary>
        /// The height of the top and bottom edges of the focus rectangle drawn byDrawFocusRect.
        /// This value is in pixels.
        /// Windows 2000:  This value is not supported.
        /// </summary>
        SM_CYFOCUSBORDER = 84,

        /// <summary>
        /// This value is the same as SM_CYSIZEFRAME.
        /// </summary>
        SM_CYFRAME = 33,

        /// <summary>
        /// The height of the client area for a full-screen window on the primary display monitor, in pixels.
        /// To get the coordinates of the portion of the screen not obscured by the system taskbar or by application desktop toolbars,
        /// call the SystemParametersInfo function with the SPI_GETWORKAREA value.
        /// </summary>
        SM_CYFULLSCREEN = 17,

        /// <summary>
        /// The height of a horizontal scroll bar, in pixels.
        /// </summary>
        SM_CYHSCROLL = 3,

        /// <summary>
        /// The default height of an icon, in pixels. The LoadIcon function can load only icons with the dimensions SM_CXICON and SM_CYICON.
        /// </summary>
        SM_CYICON = 12,

        /// <summary>
        /// The height of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size
        /// SM_CXICONSPACING by SM_CYICONSPACING when arranged. This value is always greater than or equal to SM_CYICON.
        /// </summary>
        SM_CYICONSPACING = 39,

        /// <summary>
        /// For double byte character set versions of the system, this is the height of the Kanji window at the bottom of the screen, in pixels.
        /// </summary>
        SM_CYKANJIWINDOW = 18,

        /// <summary>
        /// The default height, in pixels, of a maximized top-level window on the primary display monitor.
        /// </summary>
        SM_CYMAXIMIZED = 62,

        /// <summary>
        /// The default maximum height of a window that has a caption and sizing borders, in pixels. This metric refers to the entire desktop.
        /// The user cannot drag the window frame to a size larger than these dimensions. A window can override this value by processing
        /// the WM_GETMINMAXINFO message.
        /// </summary>
        SM_CYMAXTRACK = 60,

        /// <summary>
        /// The height of a single-line menu bar, in pixels.
        /// </summary>
        SM_CYMENU = 15,

        /// <summary>
        /// The height of the default menu check-mark bitmap, in pixels.
        /// </summary>
        SM_CYMENUCHECK = 72,

        /// <summary>
        /// The height of menu bar buttons, such as the child window close button that is used in the multiple document interface, in pixels.
        /// </summary>
        SM_CYMENUSIZE = 55,

        /// <summary>
        /// The minimum height of a window, in pixels.
        /// </summary>
        SM_CYMIN = 29,

        /// <summary>
        /// The height of a minimized window, in pixels.
        /// </summary>
        SM_CYMINIMIZED = 58,

        /// <summary>
        /// The height of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged.
        /// This value is always greater than or equal to SM_CYMINIMIZED.
        /// </summary>
        SM_CYMINSPACING = 48,

        /// <summary>
        /// The minimum tracking height of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions.
        /// A window can override this value by processing the WM_GETMINMAXINFO message.
        /// </summary>
        SM_CYMINTRACK = 35,

        /// <summary>
        /// The height of the screen of the primary display monitor, in pixels. This is the same value obtained by calling
        /// GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, VERTRES).
        /// </summary>
        SM_CYSCREEN = 1,

        /// <summary>
        /// The height of a button in a window caption or title bar, in pixels.
        /// </summary>
        SM_CYSIZE = 31,

        /// <summary>
        /// The thickness of the sizing border around the perimeter of a window that can be resized, in pixels.
        /// SM_CXSIZEFRAME is the width of the horizontal border, and SM_CYSIZEFRAME is the height of the vertical border.
        /// This value is the same as SM_CYFRAME.
        /// </summary>
        SM_CYSIZEFRAME = 33,

        /// <summary>
        /// The height of a small caption, in pixels.
        /// </summary>
        SM_CYSMCAPTION = 51,

        /// <summary>
        /// The recommended height of a small icon, in pixels. Small icons typically appear in window captions and in small icon view.
        /// </summary>
        SM_CYSMICON = 50,

        /// <summary>
        /// The height of small caption buttons, in pixels.
        /// </summary>
        SM_CYSMSIZE = 53,

        /// <summary>
        /// The height of the virtual screen, in pixels. The virtual screen is the bounding rectangle of all display monitors.
        /// The SM_YVIRTUALSCREEN metric is the coordinates for the top of the virtual screen.
        /// </summary>
        SM_CYVIRTUALSCREEN = 79,

        /// <summary>
        /// The height of the arrow bitmap on a vertical scroll bar, in pixels.
        /// </summary>
        SM_CYVSCROLL = 20,

        /// <summary>
        /// The height of the thumb box in a vertical scroll bar, in pixels.
        /// </summary>
        SM_CYVTHUMB = 9,

        /// <summary>
        /// Nonzero if User32.dll supports DBCS; otherwise, 0.
        /// </summary>
        SM_DBCSENABLED = 42,

        /// <summary>
        /// Nonzero if the debug version of User.exe is installed; otherwise, 0.
        /// </summary>
        SM_DEBUG = 22,

        /// <summary>
        /// Nonzero if the current operating system is Windows 7 or Windows Server 2008 R2 and the Tablet PC Input
        /// service is started; otherwise, 0. The return value is a bitmask that specifies the type of digitizer input supported by the device.
        /// For more information, see Remarks.
        /// Windows Server 2008, Windows Vista, and Windows XP/2000:  This value is not supported.
        /// </summary>
        SM_DIGITIZER = 94,

        /// <summary>
        /// Nonzero if Input Method Manager/Input Method Editor features are enabled; otherwise, 0.
        /// SM_IMMENABLED indicates whether the system is ready to use a Unicode-based IME on a Unicode application.
        /// To ensure that a language-dependent IME works, check SM_DBCSENABLED and the system ANSI code page.
        /// Otherwise the ANSI-to-Unicode conversion may not be performed correctly, or some components like fonts
        /// or registry settings may not be present.
        /// </summary>
        SM_IMMENABLED = 82,

        /// <summary>
        /// Nonzero if there are digitizers in the system; otherwise, 0. SM_MAXIMUMTOUCHES returns the aggregate maximum of the
        /// maximum number of contacts supported by every digitizer in the system. If the system has only single-touch digitizers,
        /// the return value is 1. If the system has multi-touch digitizers, the return value is the number of simultaneous contacts
        /// the hardware can provide. Windows Server 2008, Windows Vista, and Windows XP/2000:  This value is not supported.
        /// </summary>
        SM_MAXIMUMTOUCHES = 95,

        /// <summary>
        /// Nonzero if the current operating system is the Windows XP, Media Center Edition, 0 if not.
        /// </summary>
        SM_MEDIACENTER = 87,

        /// <summary>
        /// Nonzero if drop-down menus are right-aligned with the corresponding menu-bar item; 0 if the menus are left-aligned.
        /// </summary>
        SM_MENUDROPALIGNMENT = 40,

        /// <summary>
        /// Nonzero if the system is enabled for Hebrew and Arabic languages, 0 if not.
        /// </summary>
        SM_MIDEASTENABLED = 74,

        /// <summary>
        /// Nonzero if a mouse is installed; otherwise, 0. This value is rarely zero, because of support for virtual mice and because
        /// some systems detect the presence of the port instead of the presence of a mouse.
        /// </summary>
        SM_MOUSEPRESENT = 19,

        /// <summary>
        /// Nonzero if a mouse with a horizontal scroll wheel is installed; otherwise 0.
        /// </summary>
        SM_MOUSEHORIZONTALWHEELPRESENT = 91,

        /// <summary>
        /// Nonzero if a mouse with a vertical scroll wheel is installed; otherwise 0.
        /// </summary>
        SM_MOUSEWHEELPRESENT = 75,

        /// <summary>
        /// The least significant bit is set if a network is present; otherwise, it is cleared. The other bits are reserved for future use.
        /// </summary>
        SM_NETWORK = 63,

        /// <summary>
        /// Nonzero if the Microsoft Windows for Pen computing extensions are installed; zero otherwise.
        /// </summary>
        SM_PENWINDOWS = 41,

        /// <summary>
        /// This system metric is used in a Terminal Services environment to determine if the current Terminal Server session is
        /// being remotely controlled. Its value is nonzero if the current session is remotely controlled; otherwise, 0.
        /// You can use terminal services management tools such as Terminal Services Manager (tsadmin.msc) and shadow.exe to
        /// control a remote session. When a session is being remotely controlled, another user can view the contents of that session
        /// and potentially interact with it.
        /// </summary>
        SM_REMOTECONTROL = 0x2001,

        /// <summary>
        /// This system metric is used in a Terminal Services environment. If the calling process is associated with a Terminal Services
        /// client session, the return value is nonzero. If the calling process is associated with the Terminal Services console session,
        /// the return value is 0.
        /// Windows Server 2003 and Windows XP:  The console session is not necessarily the physical console.
        /// For more information, seeWTSGetActiveConsoleSessionId.
        /// </summary>
        SM_REMOTESESSION = 0x1000,

        /// <summary>
        /// Nonzero if all the display monitors have the same color format, otherwise, 0. Two displays can have the same bit depth,
        /// but different color formats. For example, the red, green, and blue pixels can be encoded with different numbers of bits,
        /// or those bits can be located in different places in a pixel color value.
        /// </summary>
        SM_SAMEDISPLAYFORMAT = 81,

        /// <summary>
        /// This system metric should be ignored; it always returns 0.
        /// </summary>
        SM_SECURE = 44,

        /// <summary>
        /// The build number if the system is Windows Server 2003 R2; otherwise, 0.
        /// </summary>
        SM_SERVERR2 = 89,

        /// <summary>
        /// Nonzero if the user requires an application to present information visually in situations where it would otherwise present
        /// the information only in audible form; otherwise, 0.
        /// </summary>
        SM_SHOWSOUNDS = 70,

        /// <summary>
        /// Nonzero if the current session is shutting down; otherwise, 0. Windows 2000:  This value is not supported.
        /// </summary>
        SM_SHUTTINGDOWN = 0x2000,

        /// <summary>
        /// Nonzero if the computer has a low-end (slow) processor; otherwise, 0.
        /// </summary>
        SM_SLOWMACHINE = 73,

        /// <summary>
        /// Nonzero if the current operating system is Windows 7 Starter Edition, Windows Vista Starter, or Windows XP Starter Edition; otherwise, 0.
        /// </summary>
        SM_STARTER = 88,

        /// <summary>
        /// Nonzero if the meanings of the left and right mouse buttons are swapped; otherwise, 0.
        /// </summary>
        SM_SWAPBUTTON = 23,

        /// <summary>
        /// Nonzero if the current operating system is the Windows XP Tablet PC edition or if the current operating system is Windows Vista
        /// or Windows 7 and the Tablet PC Input service is started; otherwise, 0. The SM_DIGITIZER setting indicates the type of digitizer
        /// input supported by a device running Windows 7 or Windows Server 2008 R2. For more information, see Remarks.
        /// </summary>
        SM_TABLETPC = 86,

        /// <summary>
        /// The coordinates for the left side of the virtual screen. The virtual screen is the bounding rectangle of all display monitors.
        /// The SM_CXVIRTUALSCREEN metric is the width of the virtual screen.
        /// </summary>
        SM_XVIRTUALSCREEN = 76,

        /// <summary>
        /// The coordinates for the top of the virtual screen. The virtual screen is the bounding rectangle of all display monitors.
        /// The SM_CYVIRTUALSCREEN metric is the height of the virtual screen.
        /// </summary>
        SM_YVIRTUALSCREEN = 77,
    }


    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(SystemMetric smIndex);


    [Flags]
    public enum SetWindowPosFlags : uint
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
        /// </summary>
        SWP_ASYNCWINDOWPOS = 0x4000,

        /// <summary>
        ///     Prevents generation of the WM_SYNCPAINT message.
        /// </summary>
        SWP_DEFERERASE = 0x2000,

        /// <summary>
        ///     Draws a frame (defined in the window's class description) around the window.
        /// </summary>
        SWP_DRAWFRAME = 0x0020,

        /// <summary>
        ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
        /// </summary>
        SWP_FRAMECHANGED = 0x0020,

        /// <summary>
        ///     Hides the window.
        /// </summary>
        SWP_HIDEWINDOW = 0x0080,

        /// <summary>
        ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
        /// </summary>
        SWP_NOACTIVATE = 0x0010,

        /// <summary>
        ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
        /// </summary>
        SWP_NOCOPYBITS = 0x0100,

        /// <summary>
        ///     Retains the current position (ignores X and Y parameters).
        /// </summary>
        SWP_NOMOVE = 0x0002,

        /// <summary>
        ///     Does not change the owner window's position in the Z order.
        /// </summary>
        SWP_NOOWNERZORDER = 0x0200,

        /// <summary>
        ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
        /// </summary>
        SWP_NOREDRAW = 0x0008,

        /// <summary>
        ///     Same as the SWP_NOOWNERZORDER flag.
        /// </summary>
        SWP_NOREPOSITION = 0x0200,

        /// <summary>
        ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        /// </summary>
        SWP_NOSENDCHANGING = 0x0400,

        /// <summary>
        ///     Retains the current size (ignores the cx and cy parameters).
        /// </summary>
        SWP_NOSIZE = 0x0001,

        /// <summary>
        ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
        /// </summary>
        SWP_NOZORDER = 0x0004,

        /// <summary>
        ///     Displays the window.
        /// </summary>
        SWP_SHOWWINDOW = 0x0040,

        // ReSharper restore InconsistentNaming
    }


    /// <summary>
    ///     Special window handles
    /// </summary>
    public enum SpecialWindowHandles
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        ///     Places the window at the top of the Z order.
        /// </summary>
        HWND_TOP = 0,
        /// <summary>
        ///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
        /// </summary>
        HWND_BOTTOM = 1,
        /// <summary>
        ///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
        /// </summary>
        HWND_TOPMOST = -1,
        /// <summary>
        ///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
        /// </summary>
        HWND_NOTOPMOST = -2
        // ReSharper restore InconsistentNaming
    }


    /// <summary>
    ///     Changes the size, position, and Z order of a child, pop-up, or top-level window. These windows are ordered
    ///     according to their appearance on the screen. The topmost window receives the highest rank and is the first window
    ///     in the Z order.
    ///     <para>See https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx for more information.</para>
    /// </summary>
    /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br />A handle to the window.</param>
    /// <param name="hWndInsertAfter">
    ///     C++ ( hWndInsertAfter [in, optional]. Type: HWND )<br />A handle to the window to precede the positioned window in
    ///     the Z order. This parameter must be a window handle or one of the following values.
    ///     <list type="table">
    ///     <itemheader>
    ///         <term>HWND placement</term><description>Window to precede placement</description>
    ///     </itemheader>
    ///     <item>
    ///         <term>HWND_BOTTOM ((HWND)1)</term>
    ///         <description>
    ///         Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost
    ///         window, the window loses its topmost status and is placed at the bottom of all other windows.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>HWND_NOTOPMOST ((HWND)-2)</term>
    ///         <description>
    ///         Places the window above all non-topmost windows (that is, behind all topmost windows). This
    ///         flag has no effect if the window is already a non-topmost window.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>HWND_TOP ((HWND)0)</term><description>Places the window at the top of the Z order.</description>
    ///     </item>
    ///     <item>
    ///         <term>HWND_TOPMOST ((HWND)-1)</term>
    ///         <description>
    ///         Places the window above all non-topmost windows. The window maintains its topmost position
    ///         even when it is deactivated.
    ///         </description>
    ///     </item>
    ///     </list>
    ///     <para>For more information about how this parameter is used, see the following Remarks section.</para>
    /// </param>
    /// <param name="X">C++ ( X [in]. Type: int )<br />The new position of the left side of the window, in client coordinates.</param>
    /// <param name="Y">C++ ( Y [in]. Type: int )<br />The new position of the top of the window, in client coordinates.</param>
    /// <param name="cx">C++ ( cx [in]. Type: int )<br />The new width of the window, in pixels.</param>
    /// <param name="cy">C++ ( cy [in]. Type: int )<br />The new height of the window, in pixels.</param>
    /// <param name="uFlags">
    ///     C++ ( uFlags [in]. Type: UINT )<br />The window sizing and positioning flags. This parameter can be a combination
    ///     of the following values.
    ///     <list type="table">
    ///     <itemheader>
    ///         <term>HWND sizing and positioning flags</term>
    ///         <description>Where to place and size window. Can be a combination of any</description>
    ///     </itemheader>
    ///     <item>
    ///         <term>SWP_ASYNCWINDOWPOS (0x4000)</term>
    ///         <description>
    ///         If the calling thread and the thread that owns the window are attached to different input
    ///         queues, the system posts the request to the thread that owns the window. This prevents the calling
    ///         thread from blocking its execution while other threads process the request.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_DEFERERASE (0x2000)</term>
    ///         <description>Prevents generation of the WM_SYNCPAINT message. </description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_DRAWFRAME (0x0020)</term>
    ///         <description>Draws a frame (defined in the window's class description) around the window.</description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_FRAMECHANGED (0x0020)</term>
    ///         <description>
    ///         Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message
    ///         to the window, even if the window's size is not being changed. If this flag is not specified,
    ///         WM_NCCALCSIZE is sent only when the window's size is being changed
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_HIDEWINDOW (0x0080)</term><description>Hides the window.</description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_NOACTIVATE (0x0010)</term>
    ///         <description>
    ///         Does not activate the window. If this flag is not set, the window is activated and moved to
    ///         the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
    ///         parameter).
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_NOCOPYBITS (0x0100)</term>
    ///         <description>
    ///         Discards the entire contents of the client area. If this flag is not specified, the valid
    ///         contents of the client area are saved and copied back into the client area after the window is sized or
    ///         repositioned.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_NOMOVE (0x0002)</term>
    ///         <description>Retains the current position (ignores X and Y parameters).</description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_NOOWNERZORDER (0x0200)</term>
    ///         <description>Does not change the owner window's position in the Z order.</description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_NOREDRAW (0x0008)</term>
    ///         <description>
    ///         Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies
    ///         to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
    ///         parent window uncovered as a result of the window being moved. When this flag is set, the application
    ///         must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_NOREPOSITION (0x0200)</term><description>Same as the SWP_NOOWNERZORDER flag.</description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_NOSENDCHANGING (0x0400)</term>
    ///         <description>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_NOSIZE (0x0001)</term>
    ///         <description>Retains the current size (ignores the cx and cy parameters).</description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_NOZORDER (0x0004)</term>
    ///         <description>Retains the current Z order (ignores the hWndInsertAfter parameter).</description>
    ///     </item>
    ///     <item>
    ///         <term>SWP_SHOWWINDOW (0x0040)</term><description>Displays the window.</description>
    ///     </item>
    ///     </list>
    /// </param>
    /// <returns><c>true</c> or nonzero if the function succeeds, <c>false</c> or zero otherwise or if function fails.</returns>
    /// <remarks>
    ///     <para>
    ///     As part of the Vista re-architecture, all services were moved off the interactive desktop into Session 0.
    ///     hwnd and window manager operations are only effective inside a session and cross-session attempts to manipulate
    ///     the hwnd will fail. For more information, see The Windows Vista Developer Story: Application Compatibility
    ///     Cookbook.
    ///     </para>
    ///     <para>
    ///     If you have changed certain window data using SetWindowLong, you must call SetWindowPos for the changes to
    ///     take effect. Use the following combination for uFlags: SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |
    ///     SWP_FRAMECHANGED.
    ///     </para>
    ///     <para>
    ///     A window can be made a topmost window either by setting the hWndInsertAfter parameter to HWND_TOPMOST and
    ///     ensuring that the SWP_NOZORDER flag is not set, or by setting a window's position in the Z order so that it is
    ///     above any existing topmost windows. When a non-topmost window is made topmost, its owned windows are also made
    ///     topmost. Its owners, however, are not changed.
    ///     </para>
    ///     <para>
    ///     If neither the SWP_NOACTIVATE nor SWP_NOZORDER flag is specified (that is, when the application requests that
    ///     a window be simultaneously activated and its position in the Z order changed), the value specified in
    ///     hWndInsertAfter is used only in the following circumstances.
    ///     </para>
    ///     <list type="bullet">
    ///     <item>Neither the HWND_TOPMOST nor HWND_NOTOPMOST flag is specified in hWndInsertAfter. </item>
    ///     <item>The window identified by hWnd is not the active window. </item>
    ///     </list>
    ///     <para>
    ///     An application cannot activate an inactive window without also bringing it to the top of the Z order.
    ///     Applications can change an activated window's position in the Z order without restrictions, or it can activate
    ///     a window and then move it to the top of the topmost or non-topmost windows.
    ///     </para>
    ///     <para>
    ///     If a topmost window is repositioned to the bottom (HWND_BOTTOM) of the Z order or after any non-topmost
    ///     window, it is no longer topmost. When a topmost window is made non-topmost, its owners and its owned windows
    ///     are also made non-topmost windows.
    ///     </para>
    ///     <para>
    ///     A non-topmost window can own a topmost window, but the reverse cannot occur. Any window (for example, a
    ///     dialog box) owned by a topmost window is itself made a topmost window, to ensure that all owned windows stay
    ///     above their owner.
    ///     </para>
    ///     <para>
    ///     If an application is not in the foreground, and should be in the foreground, it must call the
    ///     SetForegroundWindow function.
    ///     </para>
    ///     <para>
    ///     To use SetWindowPos to bring a window to the top, the process that owns the window must have
    ///     SetForegroundWindow permission.
    ///     </para>
    /// </remarks>
    [DllImport("user32.dll", SetLastError=true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);


    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }


    public enum MonitorOptions : uint
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }


    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);


    // size of a device name string
    const int CCHDEVICENAME = 32;
    const uint MONITORINFOF_PRIMARY = 1;

    /// <summary>
    /// The MONITORINFOEX structure contains information about a display monitor.
    /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
    /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name
    /// for the display monitor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MonitorInfoEx
    {
        /// <summary>
        /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function.
        /// Doing so lets the function determine the type of structure you are passing to it.
        /// </summary>
        public int Size;

        /// <summary>
        /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
        /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public RectStruct Monitor;

        /// <summary>
        /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications,
        /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor.
        /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars.
        /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public RectStruct WorkArea;

        /// <summary>
        /// The attributes of the display monitor.
        ///
        /// This member can be the following value:
        ///   1 : MONITORINFOF_PRIMARY
        /// </summary>
        public uint Flags;

        /// <summary>
        /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name,
        /// and so can save some bytes by using a MONITORINFO structure.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        public string DeviceName;

        public void Init()
        {
            this.Size = 40 + 2 * CCHDEVICENAME;
            this.DeviceName = string.Empty;
        }
    }


    /// <summary>
    /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/dd162897%28VS.85%29.aspx"/>
    /// <remarks>
    /// By convention, the right and bottom edges of the rectangle are normally considered exclusive.
    /// In other words, the pixel whose coordinates are ( right, bottom ) lies immediately outside of the the rectangle.
    /// For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including,
    /// the right column and bottom row of pixels. This structure is identical to the RECTL structure.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct RectStruct
    {
        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Left;

        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Top;

        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Right;

        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Bottom;

        public override string ToString() { return string.Format("Left: {0}, Top: {1}, Right: {2}, Bottom: {3}", Left, Top, Right, Bottom); }
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);


    [Flags()]
    public enum DM : int
    {
        Orientation = 0x1,
        PaperSize = 0x2,
        PaperLength = 0x4,
        PaperWidth = 0x8,
        Scale = 0x10,
        Position = 0x20,
        NUP = 0x40,
        DisplayOrientation = 0x80,
        Copies = 0x100,
        DefaultSource = 0x200,
        PrintQuality = 0x400,
        Color = 0x800,
        Duplex = 0x1000,
        YResolution = 0x2000,
        TTOption = 0x4000,
        Collate = 0x8000,
        FormName = 0x10000,
        LogPixels = 0x20000,
        BitsPerPixel = 0x40000,
        PelsWidth = 0x80000,
        PelsHeight = 0x100000,
        DisplayFlags = 0x200000,
        DisplayFrequency = 0x400000,
        ICMMethod = 0x800000,
        ICMIntent = 0x1000000,
        MediaType = 0x2000000,
        DitherType = 0x4000000,
        PanningWidth = 0x8000000,
        PanningHeight = 0x10000000,
        DisplayFixedOutput = 0x20000000
    }

    public struct POINTL
    {
        public Int32 x;
        public Int32 y;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        public const int CCHDEVICENAME = 32;
        public const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        [System.Runtime.InteropServices.FieldOffset(0)]
        public string dmDeviceName;

        [System.Runtime.InteropServices.FieldOffset(32)]
        public Int16 dmSpecVersion;

        [System.Runtime.InteropServices.FieldOffset(34)]
        public Int16 dmDriverVersion;

        [System.Runtime.InteropServices.FieldOffset(36)]
        public Int16 dmSize;

        [System.Runtime.InteropServices.FieldOffset(38)]
        public Int16 dmDriverExtra;

        [System.Runtime.InteropServices.FieldOffset(40)]
        public DM dmFields;

        [System.Runtime.InteropServices.FieldOffset(44)]
        Int16 dmOrientation;

        [System.Runtime.InteropServices.FieldOffset(46)]
        Int16 dmPaperSize;

        [System.Runtime.InteropServices.FieldOffset(48)]
        Int16 dmPaperLength;

        [System.Runtime.InteropServices.FieldOffset(50)]
        Int16 dmPaperWidth;

        [System.Runtime.InteropServices.FieldOffset(52)]
        Int16 dmScale;

        [System.Runtime.InteropServices.FieldOffset(54)]
        Int16 dmCopies;

        [System.Runtime.InteropServices.FieldOffset(56)]
        Int16 dmDefaultSource;

        [System.Runtime.InteropServices.FieldOffset(58)]
        Int16 dmPrintQuality;

        [System.Runtime.InteropServices.FieldOffset(44)]
        public POINTL dmPosition;

        [System.Runtime.InteropServices.FieldOffset(52)]
        public Int32 dmDisplayOrientation;

        [System.Runtime.InteropServices.FieldOffset(56)]
        public Int32 dmDisplayFixedOutput;

        [System.Runtime.InteropServices.FieldOffset(60)]
        public short dmColor; // See note below!

        [System.Runtime.InteropServices.FieldOffset(62)]
        public short dmDuplex; // See note below!

        [System.Runtime.InteropServices.FieldOffset(64)]
        public short dmYResolution;

        [System.Runtime.InteropServices.FieldOffset(66)]
        public short dmTTOption;

        [System.Runtime.InteropServices.FieldOffset(68)]
        public short dmCollate; // See note below!

        [System.Runtime.InteropServices.FieldOffset(72)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
        public string dmFormName;

        [System.Runtime.InteropServices.FieldOffset(102)]
        public Int16 dmLogPixels;

        [System.Runtime.InteropServices.FieldOffset(104)]
        public Int32 dmBitsPerPel;

        [System.Runtime.InteropServices.FieldOffset(108)]
        public Int32 dmPelsWidth;

        [System.Runtime.InteropServices.FieldOffset(112)]
        public Int32 dmPelsHeight;

        [System.Runtime.InteropServices.FieldOffset(116)]
        public Int32 dmDisplayFlags;

        [System.Runtime.InteropServices.FieldOffset(116)]
        public Int32 dmNup;

        [System.Runtime.InteropServices.FieldOffset(120)]
        public Int32 dmDisplayFrequency;

        public void Init()
        {
            this.dmDeviceName = string.Empty;
            this.dmFormName = string.Empty;
            this.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
        }
    }


    public const int ENUM_CURRENT_SETTINGS = -1;   // for EnumDisplaySettings()
    public const int ENUM_REGISTRY_SETTINGS = -2;  // for EnumDisplaySettings()


    [DllImport("user32.dll")]
    static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);


    public enum PROCESS_DPI_AWARENESS
    {
        Process_DPI_Unaware = 0,
        Process_System_DPI_Aware = 1,
        Process_Per_Monitor_DPI_Aware = 2
    }

    [DllImport("SHCore.dll", SetLastError = true)]
    static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

    [DllImport("SHCore.dll", SetLastError = true)]
    static extern void GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS awareness);


    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr GetCurrentProcess();


    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hWnd);


    [DllImport("user32.dll")]
    static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

    public enum DeviceCap
    {
        /// <summary>
        /// Device driver version
        /// </summary>
        DRIVERVERSION = 0,
        /// <summary>
        /// Device classification
        /// </summary>
        TECHNOLOGY = 2,
        /// <summary>
        /// Horizontal size in millimeters
        /// </summary>
        HORZSIZE = 4,
        /// <summary>
        /// Vertical size in millimeters
        /// </summary>
        VERTSIZE = 6,
        /// <summary>
        /// Horizontal width in pixels
        /// </summary>
        HORZRES = 8,
        /// <summary>
        /// Vertical height in pixels
        /// </summary>
        VERTRES = 10,
        /// <summary>
        /// Number of bits per pixel
        /// </summary>
        BITSPIXEL = 12,
        /// <summary>
        /// Number of planes
        /// </summary>
        PLANES = 14,
        /// <summary>
        /// Number of brushes the device has
        /// </summary>
        NUMBRUSHES = 16,
        /// <summary>
        /// Number of pens the device has
        /// </summary>
        NUMPENS = 18,
        /// <summary>
        /// Number of markers the device has
        /// </summary>
        NUMMARKERS = 20,
        /// <summary>
        /// Number of fonts the device has
        /// </summary>
        NUMFONTS = 22,
        /// <summary>
        /// Number of colors the device supports
        /// </summary>
        NUMCOLORS = 24,
        /// <summary>
        /// Size required for device descriptor
        /// </summary>
        PDEVICESIZE = 26,
        /// <summary>
        /// Curve capabilities
        /// </summary>
        CURVECAPS = 28,
        /// <summary>
        /// Line capabilities
        /// </summary>
        LINECAPS = 30,
        /// <summary>
        /// Polygonal capabilities
        /// </summary>
        POLYGONALCAPS = 32,
        /// <summary>
        /// Text capabilities
        /// </summary>
        TEXTCAPS = 34,
        /// <summary>
        /// Clipping capabilities
        /// </summary>
        CLIPCAPS = 36,
        /// <summary>
        /// Bitblt capabilities
        /// </summary>
        RASTERCAPS = 38,
        /// <summary>
        /// Length of the X leg
        /// </summary>
        ASPECTX = 40,
        /// <summary>
        /// Length of the Y leg
        /// </summary>
        ASPECTY = 42,
        /// <summary>
        /// Length of the hypotenuse
        /// </summary>
        ASPECTXY = 44,
        /// <summary>
        /// Shading and Blending caps
        /// </summary>
        SHADEBLENDCAPS = 45,

        /// <summary>
        /// Logical pixels inch in X
        /// </summary>
        LOGPIXELSX = 88,
        /// <summary>
        /// Logical pixels inch in Y
        /// </summary>
        LOGPIXELSY = 90,

        /// <summary>
        /// Number of entries in physical palette
        /// </summary>
        SIZEPALETTE = 104,
        /// <summary>
        /// Number of reserved entries in palette
        /// </summary>
        NUMRESERVED = 106,
        /// <summary>
        /// Actual color resolution
        /// </summary>
        COLORRES = 108,

        // Printing related DeviceCaps. These replace the appropriate Escapes
        /// <summary>
        /// Physical Width in device units
        /// </summary>
        PHYSICALWIDTH = 110,
        /// <summary>
        /// Physical Height in device units
        /// </summary>
        PHYSICALHEIGHT = 111,
        /// <summary>
        /// Physical Printable Area x margin
        /// </summary>
        PHYSICALOFFSETX = 112,
        /// <summary>
        /// Physical Printable Area y margin
        /// </summary>
        PHYSICALOFFSETY = 113,
        /// <summary>
        /// Scaling factor x
        /// </summary>
        SCALINGFACTORX = 114,
        /// <summary>
        /// Scaling factor y
        /// </summary>
        SCALINGFACTORY = 115,

        /// <summary>
        /// Current vertical refresh rate of the display device (for displays only) in Hz
        /// </summary>
        VREFRESH = 116,
        /// <summary>
        /// Vertical height of entire desktop in pixels
        /// </summary>
        DESKTOPVERTRES = 117,
        /// <summary>
        /// Horizontal width of entire desktop in pixels
        /// </summary>
        DESKTOPHORZRES = 118,
        /// <summary>
        /// Preferred blt alignment
        /// </summary>
        BLTALIGNMENT = 119
    }

    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);


    [Flags()]
    public enum DisplayDeviceStateFlags : int
    {
        /// <summary>The device is part of the desktop.</summary>
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,
        /// <summary>The device is part of the desktop.</summary>
        PrimaryDevice = 0x4,
        /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        MirroringDriver = 0x8,
        /// <summary>The device is VGA compatible.</summary>
        VGACompatible = 0x10,
        /// <summary>The device is removable; it cannot be the primary display.</summary>
        Removable = 0x20,
        /// <summary>The device has more display modes than its output devices support.</summary>
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000
    }


    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
        public string DeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
        public string DeviceString;

        [MarshalAs(UnmanagedType.U4)]
        public DisplayDeviceStateFlags StateFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
        public string DeviceID;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
        public string DeviceKey;

        public void Init()
        {
            this.cb = (short)Marshal.SizeOf(typeof(DISPLAY_DEVICE));
        }
    }

    [DllImport("user32.dll")]
    static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    public static extern bool SetWindowText(IntPtr hwnd, String lpString);


    public enum WindowLongFlags : int
    {
         GWL_EXSTYLE = -20,
         GWLP_HINSTANCE = -6,
         GWLP_HWNDPARENT = -8,
         GWL_ID = -12,
         GWL_STYLE = -16,
         GWL_USERDATA = -21,
         GWL_WNDPROC = -4,
         DWLP_USER = 0x8,
         DWLP_MSGRESULT = 0x0,
         DWLP_DLGPROC = 0x4
    }

    [Flags]
    public enum WindowStyles : uint
    {
        WS_OVERLAPPED      = 0x00000000,
        WS_POPUP           = 0x80000000,
        WS_CHILD           = 0x40000000,
        WS_MINIMIZE        = 0x20000000,
        WS_VISIBLE         = 0x10000000,
        WS_DISABLED        = 0x08000000,
        WS_CLIPSIBLINGS    = 0x04000000,
        WS_CLIPCHILDREN    = 0x02000000,
        WS_MAXIMIZE        = 0x01000000,
        WS_BORDER          = 0x00800000,
        WS_DLGFRAME        = 0x00400000,
        WS_VSCROLL         = 0x00200000,
        WS_HSCROLL         = 0x00100000,
        WS_SYSMENU         = 0x00080000,
        WS_THICKFRAME      = 0x00040000,
        WS_GROUP           = 0x00020000,
        WS_TABSTOP         = 0x00010000,

        WS_MINIMIZEBOX     = 0x00020000,
        WS_MAXIMIZEBOX     = 0x00010000,

        WS_CAPTION         = WS_BORDER | WS_DLGFRAME,
        WS_TILED           = WS_OVERLAPPED,
        WS_ICONIC          = WS_MINIMIZE,
        WS_SIZEBOX         = WS_THICKFRAME,
        WS_TILEDWINDOW     = WS_OVERLAPPEDWINDOW,

        WS_OVERLAPPEDWINDOW    = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_POPUPWINDOW     = WS_POPUP | WS_BORDER | WS_SYSMENU,
        WS_CHILDWINDOW     = WS_CHILD,

        //Extended Window Styles

        WS_EX_DLGMODALFRAME    = 0x00000001,
        WS_EX_NOPARENTNOTIFY   = 0x00000004,
        WS_EX_TOPMOST      = 0x00000008,
        WS_EX_ACCEPTFILES      = 0x00000010,
        WS_EX_TRANSPARENT      = 0x00000020,

//#if(WINVER >= 0x0400)
        WS_EX_MDICHILD     = 0x00000040,
        WS_EX_TOOLWINDOW       = 0x00000080,
        WS_EX_WINDOWEDGE       = 0x00000100,
        WS_EX_CLIENTEDGE       = 0x00000200,
        WS_EX_CONTEXTHELP      = 0x00000400,

        WS_EX_RIGHT        = 0x00001000,
        WS_EX_LEFT         = 0x00000000,
        WS_EX_RTLREADING       = 0x00002000,
        WS_EX_LTRREADING       = 0x00000000,
        WS_EX_LEFTSCROLLBAR    = 0x00004000,
        WS_EX_RIGHTSCROLLBAR   = 0x00000000,

        WS_EX_CONTROLPARENT    = 0x00010000,
        WS_EX_STATICEDGE       = 0x00020000,
        WS_EX_APPWINDOW    = 0x00040000,

        WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
        WS_EX_PALETTEWINDOW    = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
//#endif /* WINVER >= 0x0400 */

//#if(WIN32WINNT >= 0x0500)
        WS_EX_LAYERED      = 0x00080000,
//#endif /* WIN32WINNT >= 0x0500 */

//#if(WINVER >= 0x0500)
        WS_EX_NOINHERITLAYOUT  = 0x00100000, // Disable inheritence of mirroring by children
        WS_EX_LAYOUTRTL    = 0x00400000, // Right to left mirroring
//#endif /* WINVER >= 0x0500 */

//#if(WIN32WINNT >= 0x0500)

        WS_EX_COMPOSITED       = 0x02000000,
        WS_EX_NOACTIVATE       = 0x08000000
//#endif /* WIN32WINNT >= 0x0500 */
    }

    [DllImport("user32.dll", EntryPoint="GetWindowLong")]
    static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint="GetWindowLongPtr")]
    static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    // This static method is required because Win32 does not support GetWindowLongPtr directly
    static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
         if (IntPtr.Size == 8)
            return GetWindowLongPtr64(hWnd, nIndex);
         else
            return GetWindowLongPtr32(hWnd, nIndex);
    }

    [DllImport("user32.dll", EntryPoint="SetWindowLong")]
    static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint="SetWindowLongPtr")]
    static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8)
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        else
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }


    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", SetLastError=true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("user32.dll")]
    static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError=true)]
    static extern IntPtr SetActiveWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);
}

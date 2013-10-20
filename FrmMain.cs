#region 文档说明
/* ******************************************************************************************************
 * 文档作者：无闻
 * 创建日期：2013 年 10 月 16 日
 * 文档用途：七牛云盘主窗口
 * ------------------------------------------------------------------------------------------------------
 * 修改记录：
 * ------------------------------------------------------------------------------------------------------
 * 参考文献：
 * ******************************************************************************************************/
#endregion

#region 命名空间引用
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

using Qiniu.IO;
using Qiniu.RPC;
using Qiniu.RS;
using Qiniu.RSF;
using Qiniu.Conf;

using Newtonsoft.Json;

using CharmCommonMethod;
using CharmControlLibrary;
#endregion

namespace QiNiuDrive
{
    #region 枚举
    /// <summary>
    /// 过滤类型：前缀，后缀，全匹配
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// 过滤类型：前缀
        /// </summary>
        Prefix,
        /// <summary>
        /// 过滤类型：后缀
        /// </summary>
        Suffix,
        /// <summary>
        /// 过滤类型：全匹配
        /// </summary>
        FullMatch
    }
    #endregion

    #region 结构
    /// <summary>
    /// 同步文件
    /// </summary>
    public struct SyncFile
    {
        public string Name;
        public long Timestamp;

        public SyncFile(string name, long timestamp)
        {
            this.Name = name;
            this.Timestamp = timestamp;
        }
    }

    /// <summary>
    /// 过滤器
    /// </summary>
    public struct Filter
    {
        public FilterType Type;
        public string Name;

        public Filter(string name, FilterType type)
        {
            this.Name = name;
            this.Type = type;
        }
    }

    /// <summary>
    /// 修改文件
    /// </summary>
    public struct ChangeFile
    {
        public string OldName;
        public string NewName;

        public ChangeFile(string oldName, string newName)
        {
            this.OldName = oldName;
            this.NewName = newName;
        }
    }
    #endregion

    // 七牛云盘主窗口
    public partial class FrmMain : Form
    {
        #region 常量
        private const string APP_NAME = "七牛云盘";              // 软件名称
        private const string APP_VER = "v0.1.2";                // 软件版本
        private const int UPDATE_VERSION = 201310200;           // 更新版本
        private const string APP_CFG_PATH = "Config\\app.ini";  // 软件配置路径
        private const int TITLE_HEIGHT = 30;                    // 标题栏高度
        const int MENU_WIDTH = 90;                              // 菜单栏宽度
        #endregion

        #region 字段
        #region 主窗口
        // * 私有字段 *
        private string[] mMenuNames;                // 菜单项名称
        private int mMenuSelectedIndex;             // 菜单栏现行选中项索引
        private string mStatusText;                 // 状态文本
        private bool mIsLoadFinished;               // 指示程序是否加载完毕
        private bool mIsUpdateChecked;              // 指示是否完成检查更新

        // * 用户控件 *
        private List<CharmControl> mCharmControls;          // Charm 控件集合
        private readonly ToolTip mToolTip = new ToolTip();  // 工具提示文本控件
        private NotifyIcon mNotifyIcon;                     // 托盘图标
        private CharmButton mBtnApply;                      // 应用按钮
        private CharmMenu mTaryMenu;                        // 托盘菜单

        // * 七牛 *
        private RSFClient mRsfClient;
        private RSClient mRsClient;
        private List<SyncFile> mServerFileList;     // 服务器文件列表
        private List<SyncFile> mLocalFileList;      // 本地文件列表
        private List<string> mLocalFileCache;       // 本地文件缓存
        private List<ChangeFile> mChangeFileList;   // 修改文件列表
        private PutRet mPutRet;                     // 上传返回结果
        #endregion

        #region 同步设置
        // * 私有字段 *
        private bool mIsVaildSyncDir;           // 指示是否设置有效同步目录
        private bool mIsHasKeys;                // 指示是否填写密钥
        private bool mIsVaildKeys;              // 指示密钥是否有效
        private bool mIsVaildBucket;            // 指示空间名称是否有效
        private bool mIsNeedVerifyAuth;         // 指示是否需要验证授权
        private string mSyncDir;                // 同步目录
        private int mSyncCycle;                 // 同步周期
        private string mBucket = string.Empty;  // 空间名称
        private bool mIsPrivateBucket;          // 指示是否为私有空间
        private bool mIsDonePut;                // 指示是否完成上传
        private bool mIsSyncNow;                // 指示是否立即同步
        private bool mIsSyncing;                // 指示是否正在同步
        private FileSystemWatcher mFileWatcher; // 文件监视器

        // * 用户控件 *
        private List<Control> mSyncSettingControls;             // 同步设置控件集合
        // 0-同步目录文本框；1-同步周期文本框；2-AccessKey 文本框；3-SecretKey 文本框；4-空间名称文本框
        private List<CharmControl> mSyncSettingCharmControls;   // 同步设置面板 Charm 控件集合
        // 4-私有空间检查框
        #endregion

        #region 高级设置
        // * 私有字段 *
        private List<Filter> mFilterList; // 过滤器列表

        // * 用户控件 *
        private List<CharmControl> mAdvancedSettingCharmControls;   // 高级设置面板 Charm 控件集合
        #endregion

        #region 关于
        private List<CharmControl> mAboutCharmControls; // 关于面板 Charm 控件集合
        #endregion
        #endregion

        #region 窗体事件
        // 窗体构造方法
        public FrmMain()
        {
            // 初始化组件
            InitializeComponent();
            // 设置双缓冲模式
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | //不擦除背景 ,减少闪烁
                               ControlStyles.OptimizedDoubleBuffer | //双缓冲
                               ControlStyles.UserPaint, //使用自定义的重绘事件,减少闪烁
                               true);
            this.UpdateStyles();
            // 初始化设置
            InitializeSetting();
        }

        // 窗体即将关闭事件
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 释放系统资源
            mNotifyIcon.Dispose();
        }

        // 窗体鼠标单击事件
        private void frmMain_MouseClick(object sender, MouseEventArgs e)
        {
            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 0: // 同步设置
                    CharmControl.MouseClickEvent(e, mSyncSettingCharmControls);
                    break;
                case 1: // 高级设置
                    CharmControl.MouseClickEvent(e, mAdvancedSettingCharmControls);
                    break;
                case 3: // 关于
                    CharmControl.MouseClickEvent(e, mAboutCharmControls);
                    break;
            }
            #endregion

            // 调用事件
            CharmControl.MouseClickEvent(e, mCharmControls);
        }

        // 窗体鼠标按下事件
        private void frmMain_MouseDown(object sender, MouseEventArgs e)
        {
            // 只响应鼠标左键
            if (e.Button == MouseButtons.Left)
            {
                #region 菜单栏事件处理
                // 轮询菜单项
                for (int i = 0; i < mMenuNames.Length; i++)
                {
                    // 判断是否为现行选中项且未被激活
                    if (mMenuSelectedIndex == i || e.X <= 1 || e.X >= 91 ||
                        e.Y <= TITLE_HEIGHT + 1 + i * 26 || e.Y >= TITLE_HEIGHT + 5 + (i + 1) * 26)
                        continue;

                    mMenuSelectedIndex = i;     // 设置菜单栏现行选中项索引
                    ShowPanels();               // 设置控件可见性

                    // 重绘菜单栏区域
                    this.Invalidate();
                    //this.Invalidate(new Rectangle(1, 23, 90, mMenuNames.Length * 26));      
                    return;
                }
                #endregion
            }

            // 指示是否捕捉到事件
            bool isCaptureEvent = false;

            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 0: // 同步设置
                    isCaptureEvent = CharmControl.MouseDownEvent(e, mSyncSettingCharmControls, this);
                    break;
                case 1: // 高级设置
                    isCaptureEvent = CharmControl.MouseDownEvent(e, mAdvancedSettingCharmControls, this);
                    break;
                case 3: // 关于
                    isCaptureEvent = CharmControl.MouseDownEvent(e, mAboutCharmControls, this);
                    break;
            }
            #endregion

            // 判断之前是否已捕捉到事件
            if (isCaptureEvent) return;

            // 调用事件
            if (!CharmControl.MouseDownEvent(e, mCharmControls, this))
                APIOperation.MoveNoBorderForm(this, e);
        }

        // 窗体鼠标移动事件
        private void frmMain_MouseMove(object sender, MouseEventArgs e)
        {
            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 0: // 同步设置
                    CharmControl.MouseMoveEvent(e, mSyncSettingCharmControls, this, mToolTip);
                    break;
                case 1: // 高级设置
                    CharmControl.MouseMoveEvent(e, mAdvancedSettingCharmControls, this, mToolTip);
                    break;
                case 3: // 关于
                    CharmControl.MouseMoveEvent(e, mAboutCharmControls, this, mToolTip);
                    break;
            }
            #endregion

            // 调用事件
            CharmControl.MouseMoveEvent(e, mCharmControls, this, mToolTip);
        }

        // 窗体鼠标弹起事件
        private void frmMain_MouseUp(object sender, MouseEventArgs e)
        {
            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 0: // 同步设置
                    CharmControl.MouseUpEvent(e, mSyncSettingCharmControls, this);
                    break;
                case 1: // 高级设置
                    CharmControl.MouseUpEvent(e, mAdvancedSettingCharmControls, this);
                    break;
                case 3: // 关于
                    CharmControl.MouseUpEvent(e, mAboutCharmControls, this);
                    break;
            }
            #endregion

            // 调用事件
            CharmControl.MouseUpEvent(e, mCharmControls, this);
        }
        #endregion

        #region  重载事件
        // 控件重绘事件
        protected override void OnPaint(PaintEventArgs e)
        {
            // 获取绘制对象
            Graphics g = e.Graphics;

            // 绘制状态文本
            g.DrawString("（" + mStatusText + "）",
                new Font("微软雅黑", 10, FontStyle.Bold), Brushes.White, new Point(150, 6));

            #region 绘制菜单栏
            // 轮询菜单项
            for (int i = 0; i < mMenuNames.Length; i++)
            {
                // 判断是否为现行选中项
                if (mMenuSelectedIndex == i)
                {
                    g.DrawImage(Properties.Resources.tab_button_setting_chosen, new Point(1, TITLE_HEIGHT + 1 + i * 26));
                    g.DrawString(mMenuNames[i], this.Font, Brushes.White, new Point(20, TITLE_HEIGHT + 5 + i * 26));
                }
                else
                    g.DrawString(mMenuNames[i], this.Font, Brushes.Black, new Point(20, TITLE_HEIGHT + 5 + i * 26));
            }
            #endregion

            #region 绘制主面板
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 0: // 同步设置
                    DrawSyncSettingPanel(g);
                    CharmControl.PaintEvent(e.Graphics, mSyncSettingCharmControls);
                    break;
                case 1: // 高级设置
                    DrawAdvancedSettingPanel(g);
                    CharmControl.PaintEvent(e.Graphics, mAdvancedSettingCharmControls);
                    break;
                case 3: // 关于
                    DrawAboutPanel(g);
                    CharmControl.PaintEvent(e.Graphics, mAboutCharmControls);
                    break;
            }
            #endregion

            // 绘制控件
            CharmControl.PaintEvent(e.Graphics, mCharmControls);

            // 重置重绘索引
            //mMultipleSettingRedrawIndex = 0;
        }
        #endregion

        #region 控件事件
        #region 主窗口
        // 最小化按钮被单击事件
        private void btnMin_MouseClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        // 关闭按钮被单击事件
        private void btnClose_MouseClick(object sender, MouseEventArgs e)
        {
            CharmMessageBox msgbox;
            DialogResult result;

            if (mBtnApply.Enabled)
            {
                msgbox = new CharmMessageBox();
                result = msgbox.Show("您有修改未保存，是否保存？",
                   "操作确认", MessageBoxButtons.YesNo, CharmMessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                    mBtnApply_MouseClick(sender, e);
            }

            msgbox = new CharmMessageBox();
            result = msgbox.Show("您确定要退出 " + APP_NAME + " 吗？",
               "退出确认", MessageBoxButtons.YesNo, CharmMessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                this.Close();
        }

        // 托盘图标被单击事件
        private void mNotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            // 判断鼠标按键
            if (e.Button == MouseButtons.Left)
            {
                // 显示主界面
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.TopMost = true;
                this.TopMost = false;
            }
            else
                // 显示托盘菜单
                mTaryMenu.Show();
        }

        // 托盘菜单被单击事件
        private void mTaryMenu_MenuClick(int clickIndex)
        {
            // 判断用户单击的菜单项
            switch (clickIndex)
            {
                case 0: // 设置中心
                    mNotifyIcon_MouseClick(new object(), new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                    break;
                case 1: // 打开同步目录
                    if (mIsVaildSyncDir)
                        Process.Start(mSyncDir);
                    else
                    {
                        CharmMessageBox msgbox = new CharmMessageBox();
                        msgbox.Show("您的操作由于以下原因导致失败：\n\n" +
                            "- 未设置有效的同步目录",
                            "操作失败", MessageBoxButtons.OK, CharmMessageBoxIcon.Error);
                    }
                    break;
                case 3: // 立即同步
                    mIsSyncNow = true;
                    break;
                case 5: // 关于
                    mMenuSelectedIndex = 3;
                    ShowPanels();
                    this.Invalidate();
                    mNotifyIcon_MouseClick(new object(), new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                    break;
                case 6: // 退出
                    btnClose_MouseClick(new object(), new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                    break;
            }
        }

        // 确定按钮被单击事件
        private void btnMakeChange_MouseClick(object sender, MouseEventArgs e)
        {
            if (mBtnApply.Enabled)
                mBtnApply_MouseClick(sender, e);

            this.Visible = false;
        }

        // 取消按钮被单击事件
        private void btnCancel_MouseClick(object sender, MouseEventArgs e)
        {
            if (mBtnApply.Enabled)
            {
                mBtnApply.Enabled = false;
                mIsLoadFinished = false;
                LoadLocalSetting();
            }
            this.Visible = false;
        }

        // 应用按钮被单击事件
        private void mBtnApply_MouseClick(object sender, MouseEventArgs e)
        {
            mBtnApply.Enabled = false;
            this.Invalidate(mBtnApply.ClientRectangle); // 重绘控件

            #region 同步设置
            IniOperation.WriteValue(APP_CFG_PATH, "setting", "sync_dir", ((CharmTextBox)mSyncSettingControls[0]).Text);
            IniOperation.WriteValue(APP_CFG_PATH, "setting", "sync_cycle", ((CharmTextBox)mSyncSettingControls[1]).Text);
            IniOperation.WriteValue(APP_CFG_PATH, "setting", "bucket", ((CharmTextBox)mSyncSettingControls[4]).Text);
            IniOperation.WriteValue(APP_CFG_PATH, "setting", "private_bucket", (mSyncSettingCharmControls[4]).Checked.ToString());

            string accessKey = ((CharmTextBox)mSyncSettingControls[2]).Text;
            string secretKey = ((CharmTextBox)mSyncSettingControls[3]).Text;

            if (accessKey.Length > 0 && secretKey.Length > 0)
            {
                string keys = BasicMethod.DesEncrypt(accessKey, "QWERTYUI") + "|" +
                       BasicMethod.DesEncrypt(secretKey, "ASDFGHJK");
                StreamWriter sw = new StreamWriter("Config/KEY");
                sw.Write(keys);
                sw.Close();
            }

            #endregion

            mIsLoadFinished = false;
            LoadLocalSetting();

            if (!mIsUpdateChecked || !mIsVaildSyncDir || !mIsHasKeys)
            {
                RedrawStatusText("正在校验修改...");
                return;
            }

            RedrawStatusText("正在校验修改...");
            mIsNeedVerifyAuth = true;
        }
        #endregion

        #region 同步设置
        // 同步目录文本框文本改变事件（同步周期、AccessKey、SecretKey、空间名称文本框也关联此事件）
        private void txtSyncDir_TextChanged(object sender, EventArgs e)
        {
            // 首次启动时程序数据加载时不作响应
            if (!mIsLoadFinished || mBtnApply.Enabled) return;

            mBtnApply.Enabled = true;
            this.Invalidate(mBtnApply.ClientRectangle); // 重绘控件
        }

        // 浏览路径按钮被单击事件
        private void btnViewPath_MouseClick(object sender, MouseEventArgs e)
        {
            // 创建并实例化文件浏览对话框
            FolderBrowserDialog folderBrowserFialog = new FolderBrowserDialog
            {
                Description = "请选择下载文件保存目录",
                SelectedPath = ((CharmTextBox)mSyncSettingControls[0]).Text
            };

            // 显示对话框并判断用户是否指定新的目录
            if (folderBrowserFialog.ShowDialog() == DialogResult.OK)
                ((CharmTextBox)mSyncSettingControls[0]).Text = folderBrowserFialog.SelectedPath;    // 用户指定新的目录
        }

        // 私有空间检查框被单击事件
        private void chkPrivateBucket_MouseClick(object sender, MouseEventArgs e)
        {
            // 首次启动时程序数据加载时不作响应
            if (!mIsLoadFinished || mBtnApply.Enabled) return;

            mBtnApply.Enabled = true;
            this.Invalidate(mBtnApply.ClientRectangle); // 重绘控件
        }

        // 查看 AccessKey 按钮被单击事件
        private void btnViewAccessKey_MouseClick(object sender, MouseEventArgs e)
        {
            CharmMessageBox msgbox = new CharmMessageBox();
            msgbox.Show("尊敬的用户您好：\n" +
                        "以下为您的 AccessKey，请妥善保管！\n\n" +
                        ((CharmTextBox)mSyncSettingControls[2]).Text,
                        "查看密钥");
        }

        // 查看 SecretKey 按钮被单击事件
        private void btnViewSecretKey_MouseClick(object sender, MouseEventArgs e)
        {
            CharmMessageBox msgbox = new CharmMessageBox();
            msgbox.Show("尊敬的用户您好：\n" +
                        "以下为您的 SecretKey，请妥善保管！\n\n" +
                        ((CharmTextBox)mSyncSettingControls[3]).Text,
                        "查看密钥");
        }

        // 七牛开发平台链接标签鼠标单击事件
        private static void lblQiniuOpen_MouseClick(object sender, MouseEventArgs e)
        {
            Process.Start("https://portal.qiniu.com/");
        }

        // 文件监视器捕捉到重命名事件
        private void mFileWatcher_Renamed(object source, RenamedEventArgs e)
        {
            if (mIsSyncing) return;

            // 判断是否为目录改名
            if (Directory.Exists(e.FullPath))
                DirectoryRenameEvent(e.OldFullPath, e.FullPath);
            else
                FileRenameEvent(e.OldName, e.Name);
        }
        #endregion

        #region 高级设置
        // 重载过滤规则按钮被单击事件
        private void btnReloadFilter_MouseClick(object sender, MouseEventArgs e)
        {
            LoadFilterList();
            RedrawStatusText("过滤规则已重载");
        }
        #endregion

        #region 关于
        // GitHub 链接标签鼠标单击事件
        private static void lblGithub_MouseClick(object sender, MouseEventArgs e)
        {
            Process.Start("https://github.com/Unknwon/qiniudrive");
        }

        // 新浪链接标签鼠标单击事件
        private static void lblSina_MouseClick(object sender, MouseEventArgs e)
        {
            Process.Start("http://weibo.com/Obahua");
        }
        #endregion
        #endregion

        #region 线程
        // 全局更新检查线程
        private void GlobalUpdateCheck()
        {
            // 检查更新
            RedrawStatusText("正在检查更新...");

            try
            {
                WebClient wb = new WebClient { Proxy = null };
                var dict =
                    JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(
                        wb.DownloadString("http://drive.u.qiniudn.com/%E4%B8%83%E7%89%9B%E4%BA%91%E7%9B%98/UPDATE.json?download"));
                if (UPDATE_VERSION < (int)dict["version"])
                {
                    CharmMessageBox msgbox = new CharmMessageBox();
                    msgbox.Show("尊敬的用户您好：\n\n" +
                                "感谢您使用 " + APP_NAME + "，更高版本已经发布。\n\n" +
                                "为了获得更好的用户体验，建议您立即更新！\n\n" +
                                "请到 github.com/Unknwon/qiniudrive 获取最新下载地址。",
                                "版本升级");
                }
            }
            catch (Exception e)
            {
                CharmMessageBox msgbox = new CharmMessageBox();
                msgbox.Show("检查更新失败：\n\n" +
                            e.Message,
                            "错误提示", MessageBoxButtons.OK, CharmMessageBoxIcon.Error);
            }

            // 初始化七牛服务
            mRsfClient = new RSFClient(mBucket);
            mRsClient = new RSClient();
            mServerFileList = new List<SyncFile>();
            mLocalFileList = new List<SyncFile>();
            mLocalFileCache = new List<string>();
            mChangeFileList = new List<ChangeFile>();

            RedrawStatusText("正在连接服务器...");

            // 检查授权及其它设置
            VerifyAuth();
            mIsUpdateChecked = true;

            int count = 0;

            while (true)
            {
#if(!DEBUG)
    //内存释放
                Process proc = Process.GetCurrentProcess();
                proc.MaxWorkingSet = Process.GetCurrentProcess().MaxWorkingSet;
                proc.Dispose();
#endif
                if (((mSyncCycle != 0 && count % mSyncCycle == 0) || mIsSyncNow) &&
                    mIsVaildKeys && mIsVaildBucket && mIsVaildSyncDir)
                {
                    count = 0;
                    mIsSyncing = true;
                    Sync();
                    mIsSyncNow = false;
                    mIsSyncing = false;
                    mServerFileList.Clear();
                    mLocalFileList.Clear();
                }

                // 检查是否需要验证授权
                if (mIsNeedVerifyAuth)
                {
                    mIsNeedVerifyAuth = false;
                    VerifyAuth();
                    RedrawStatusText("等待同步");
                }

                count++;
                Thread.Sleep(1000);
            }
            // ReSharper disable once FunctionNeverReturns
        }
        #endregion

        #region 方法
        // 初始化设置
        private void InitializeSetting()
        {
            // 设置窗体属性
            this.Icon = Properties.Resources.icon;
            this.Text = APP_NAME;

            // 绘制窗体背景
            DrawFormBackground();

            // 初始化菜单栏
            mMenuNames = new[] { "同步设置", "高级设置", "网络设置", "关于" };

            #region 创建窗体组件
            // 创建最小化系统按钮
            CharmSysButton btnMin = new CharmSysButton
            {
                SysButtonType = SysButtonType.Minimum,
                ToolTipText = "最小化",
                Location = new Point(this.Width - 75, 1)
            };
            // 创建关闭系统按钮
            CharmSysButton btnClose = new CharmSysButton
            {
                SysButtonType = SysButtonType.Close,
                ToolTipText = "关闭",
                Location = new Point(this.Width - 44, 1)
            };

            // 创建确定按钮
            CharmButton btnMakeChange = new CharmButton
            {
                ButtonType = ButtonType.Classic_Size_08223,
                Text = "确 定",
                Location = new Point(300, 430)
            };
            // 创建取消按钮
            CharmButton btnCancel = new CharmButton
            {
                ButtonType = ButtonType.Classic_Size_08223,
                Text = "取 消",
                Location = new Point(390, 430)
            };
            // 创建应用按钮
            mBtnApply = new CharmButton
            {
                ButtonType = ButtonType.Classic_Size_08223,
                Text = "应 用",
                Location = new Point(480, 430),
                Enabled = false
            };

            // 创建控件集合
            mCharmControls = new List<CharmControl> { btnMin, btnClose, btnMakeChange, btnCancel, mBtnApply };

            // 关联控件事件
            btnMin.MouseClick += btnMin_MouseClick;
            btnClose.MouseClick += btnClose_MouseClick;
            btnMakeChange.MouseClick += btnMakeChange_MouseClick;
            btnCancel.MouseClick += btnCancel_MouseClick;
            mBtnApply.MouseClick += mBtnApply_MouseClick;
            #endregion

            #region 创建主体区域
            // 创建同步设置面板
            CreateSyncSettingPanel();
            // 创建高级设置面板
            CreateAdvancedSettingPanel();
            // 创建关于面板
            CreateAboutPanel();
            #endregion

            // 加载本地设置
            LoadLocalSetting();

            // 设置初始参数
            if (!mIsHasKeys)
                mStatusText = "未填写密钥";
            else if (!mIsVaildSyncDir)
                mStatusText = "无效的同步目录";
            else
                mStatusText = "正在初始化...";

            #region 创建菜单
            // 创建托盘菜单
            mTaryMenu = new CharmMenu();
            mTaryMenu.AddItem("设置中心", MenuItemType.TextItem,
                new Bitmap(Properties.Resources.logo, 16, 16));
            mTaryMenu.AddItem("打开同步目录", MenuItemType.TextItem);
            mTaryMenu.AddItem("", MenuItemType.Spliter);
            mTaryMenu.AddItem("立即同步", MenuItemType.TextItem);
            mTaryMenu.AddItem("", MenuItemType.Spliter);
            mTaryMenu.AddItem("关于", MenuItemType.TextItem);
            mTaryMenu.AddItem("退出", MenuItemType.TextItem);

            // 关联控件事件
            mTaryMenu.MenuClick += mTaryMenu_MenuClick;
            #endregion

            #region 创建托盘图标
            // 创建托盘图标
            mNotifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.icon,
                Text = APP_NAME + "\n" + mStatusText,
                Visible = true
            };

            // 关联控件事件
            mNotifyIcon.MouseClick += mNotifyIcon_MouseClick;
            #endregion

            #region 创建服务线程
            // 启动全局更新检查线程
            Thread globalUpdateCheckThread = new Thread(GlobalUpdateCheck) { IsBackground = true };
            globalUpdateCheckThread.Start();
            #endregion
        }

        // 绘制窗体背景
        private void DrawFormBackground()
        {
            // 加载窗体背景图像资源
            Bitmap imgBkg = new Bitmap(this.Width, this.Height);

            #region 绘制窗体内容
            using (Graphics g = Graphics.FromImage(imgBkg))
            {
                // 设置参数
                const int bottomHeight = 45;

                // 绘制标题栏背景
                SolidBrush sb = new SolidBrush(Color.DodgerBlue);
                g.FillRectangle(sb, 0, 0, this.Width, TITLE_HEIGHT);
                // 绘制窗口图标
                g.DrawImage(Properties.Resources.logo, new Rectangle(6, 5, 20, 20));
                // 绘制窗口标题
                g.DrawString("设置中心 - 七牛云盘",
                    new Font("微软雅黑", 10, FontStyle.Bold), Brushes.White, new Point(30, 6));
                // 绘制竖线
                g.DrawLine(Pens.DarkGray, MENU_WIDTH + 1, TITLE_HEIGHT, 91, this.Height);
                // 绘制菜单区
                sb.Color = Color.FromArgb(100, Color.LightGray);
                g.FillRectangle(sb, 1, TITLE_HEIGHT, 90, this.Height);
                // 绘制主面板区
                sb.Color = Color.FromArgb(255, Color.WhiteSmoke);
                g.FillRectangle(sb, MENU_WIDTH + 2, TITLE_HEIGHT, this.Width - MENU_WIDTH - 4, this.Height - TITLE_HEIGHT - bottomHeight);
                // 绘制横线
                g.DrawLine(Pens.DarkGray, MENU_WIDTH + 2, this.Height - bottomHeight, this.Width - 2, this.Height - bottomHeight);
                //// 绘制按钮区
                sb.Color = Color.FromArgb(70, Color.LightGray);
                g.FillRectangle(sb, MENU_WIDTH + 2, this.Height - bottomHeight, this.Width - 2, bottomHeight);
                // 绘制边框
                g.DrawRectangle(new Pen(Color.DimGray), 0, 0, this.Width - 1, this.Height - 1);
            }
            #endregion

            // 设置背景资源
            this.BackgroundImage = new Bitmap(imgBkg);

            // 释放系统资源
            imgBkg.Dispose();
        }

        // 加载本地设置
        private void LoadLocalSetting()
        {
            // 判断是否为初次运行程序
            if (!Directory.Exists("Config"))
                Directory.CreateDirectory("Config");

            mIsVaildKeys = true;
            mIsVaildBucket = true;

            #region 同步设置
            // 获取同步目录
            mSyncDir = IniOperation.ReadValue(APP_CFG_PATH, "setting", "sync_dir");
            if (mSyncDir.Length > 0 && Directory.Exists(mSyncDir))
            {
                ((CharmTextBox)mSyncSettingControls[0]).Text = mSyncDir;

                // 初始化文件监视器
                if (mFileWatcher == null)
                {
                    mFileWatcher = new FileSystemWatcher { IncludeSubdirectories = true };
                    mFileWatcher.Renamed += mFileWatcher_Renamed;
                }

                mFileWatcher.Path = mSyncDir;
                mFileWatcher.EnableRaisingEvents = true;
                mIsVaildSyncDir = true;
            }
            else
            {
                if (mFileWatcher != null)
                    mFileWatcher.Dispose();
                mIsVaildSyncDir = false;
            }

            // 获取同步周期
            try
            {
                mSyncCycle = Convert.ToInt32(IniOperation.ReadValue(APP_CFG_PATH, "setting", "sync_cycle"));
            }
            catch
            {
                mSyncCycle = 60;
            }

            if (mSyncCycle < 10)
                mSyncCycle = 10;
            ((CharmTextBox)mSyncSettingControls[1]).Text = Convert.ToString(mSyncCycle);

            // 获取空间名称
            mBucket = IniOperation.ReadValue(APP_CFG_PATH, "setting", "bucket");
            if (mBucket.Length > 0)
                ((CharmTextBox)mSyncSettingControls[4]).Text = mBucket;
            else
                mIsVaildBucket = false;
            // 私有空间
            if (IniOperation.ReadValue(APP_CFG_PATH, "setting", "private_bucket").Equals("True"))
            {
                mIsPrivateBucket = true;
                mSyncSettingCharmControls[4].Checked = true;
            }

            // 获取密钥
            if (File.Exists("Config/KEY"))
            {
                StreamReader sr = new StreamReader("Config/KEY");
                string[] keys = sr.ReadToEnd().Split('|');
                sr.Close();

                if (keys.Length > 0)
                {
                    Config.ACCESS_KEY = BasicMethod.DesDecrypt(keys[0], "QWERTYUI");
                    Config.SECRET_KEY = BasicMethod.DesDecrypt(keys[1], "ASDFGHJK");
                    ((CharmTextBox)mSyncSettingControls[2]).Text = Config.ACCESS_KEY;
                    ((CharmTextBox)mSyncSettingControls[3]).Text = Config.SECRET_KEY;
                    mIsHasKeys = true;
                }
            }
            #endregion

            #region 高级设置
            LoadFilterList();
            #endregion

            mIsLoadFinished = true;
        }

        // 加载过滤器列表
        private void LoadFilterList()
        {
            if (!File.Exists("Config/filter.txt")) return;

            if (mFilterList == null)
                mFilterList = new List<Filter>();
            else
                mFilterList.Clear();

            StreamReader sr = new StreamReader("Config/filter.txt");
            string[] filters = sr.ReadToEnd().Split('\n');
            sr.Close();

            foreach (string f in filters)
            {
                string name = f.TrimEnd('\r');
                if (name.Length <= 0) continue;

                int index = name.IndexOf('*');

                // 判断过滤类型
                if (index == 0) // 后缀
                    mFilterList.Add(new Filter(name.Substring(1), FilterType.Suffix));
                else if (index == name.Length - 1)  // 前缀
                    mFilterList.Add(new Filter(name.Substring(0, name.Length - 1), FilterType.Prefix));
                else    // 全匹配
                    mFilterList.Add(new Filter(name, FilterType.FullMatch));
            }
        }

        // 显示面板
        private void ShowPanels()
        {
            // 设置面板相关控件的可见性
            if (mMenuSelectedIndex == 0)   // 同步设置
                foreach (Control ctrl in mSyncSettingControls)
                    ctrl.Visible = true;
            else
                foreach (Control ctrl in mSyncSettingControls)
                    ctrl.Visible = false;
        }

        // 重绘状态文本
        private void RedrawStatusText(string text)
        {
            if (text.Length > 0)
                mStatusText = text;

            if (!mIsHasKeys)
                mStatusText = "未填写密钥";
            else if (!mIsVaildSyncDir)
                mStatusText = "无效的同步目录";
            else if (!mIsVaildBucket)
                mStatusText = "无效的空间名称";
            else if (!mIsVaildKeys)
                mStatusText = "无效的密钥";

            if (mStatusText.Length > 28)
                mStatusText = mStatusText.Substring(0, 28) + "...";
            this.Invalidate(new Rectangle(150, 6, 360, 24));
            mNotifyIcon.Text = APP_NAME + "\n" + mStatusText;
        }

        // 验证授权
        private void VerifyAuth()
        {
            if (mBucket.Length <= 0) return;

            try
            {
                mRsfClient.ListPrefix(mBucket, "", "", 1);
            }
            catch (Exception e)
            {
                CheckException(e.Message);
            }
        }

        // 同步
        private void Sync()
        {
            #region 文件重命名处理
            if (mChangeFileList.Count > 0)
            {
                RedrawStatusText("正在移动文件...");

                // 处理改名文件
                List<EntryPathPair> eppList = new List<EntryPathPair>();
                foreach (ChangeFile cf in mChangeFileList)
                    // 判断文件是否依旧存在
                    if (File.Exists(mSyncDir + "\\" + cf.NewName))
                    {
                        eppList.Add(new EntryPathPair(mBucket,
                            cf.OldName.Replace("\\", "/"), cf.NewName.Replace("\\", "/")));
                        Console.WriteLine("增加移动文件：{0} {1}", cf.OldName.Replace("\\", "/"), cf.NewName.Replace("\\", "/"));
                    }
                mRsClient.BatchMove(eppList.ToArray());
            }
            #endregion

            #region 文件删除处理
            RedrawStatusText("正在对比文件...");

            // 获取本地文件列表
            LoadLocalFiles("");

            // 获取本地文件缓存
            if (File.Exists("Data/file_cache_list"))
            {
                mLocalFileCache.Clear();

                StreamReader sr = new StreamReader("Data/file_cache_list");
                string[] files = sr.ReadToEnd().Split('\n');
                sr.Close();

                // 查找删除项
                foreach (string f in files)
                {
                    string cacheName = f.TrimEnd('\r');
                    // 判断文件是否被移动
                    if (cacheName.Length <= 0 || FindChangeFileIndex(cacheName) > -1) continue;

                    cacheName = cacheName.Replace("\\", "/");
                    if (IsNeedFilter(cacheName)) continue;

                    if (FindLocalFileIndex(cacheName) != -1)
                    {
                        mLocalFileCache.Add(cacheName);
                        continue;
                    }

                    RedrawStatusText("正在删除（上行）..." + cacheName);
                    CallRet cr = mRsClient.Delete(new EntryPath(mBucket, cacheName));
                    if (!cr.OK)
                    {
                        if (!cr.Exception.Message.Contains("612"))
                            RedrawStatusText("删除失败..." + cacheName);
                    }
                    else
                        Thread.Sleep(1000);
                }
            }

            mChangeFileList.Clear();
            #endregion

            // 拉取服务器文件列表
            bool isHasMore = true;
            string marker = string.Empty;

            while (isHasMore)
            {
                DumpRet dr = mRsfClient.ListPrefix(mBucket, "", marker);

                // 存储文件到服务器文件列表
                foreach (DumpItem item in dr.Items)
                    if (!IsNeedFilter(item.Key))
                        mServerFileList.Add(new SyncFile(item.Key, item.PutTime / 10000000));

                if (dr.Marker != null)
                    marker = dr.Marker;
                else
                    isHasMore = false;
            }

            List<string> tmpCacheList = new List<string>();
            // 文件差异对比及消除
            for (int i = 0; i < mServerFileList.Count; i++)
            {
                int index = FindLocalFileIndex(mServerFileList[i].Name);
                string curServeFileName = mServerFileList[i].Name.Replace("/", "\\");
                DateTime unixTime = new DateTime(1970, 1, 1).AddSeconds(mServerFileList[i].Timestamp);

                if (index == -1)
                {
                    RedrawStatusText("正在下载..." + mServerFileList[i].Name);
                    if (!DownloadFile("http://" + mBucket + ".u.qiniudn.com/" + mServerFileList[i].Name,
                         curServeFileName, unixTime))
                        RedrawStatusText("下载失败..." + mServerFileList[i].Name);
                }
                else if (mServerFileList[i].Timestamp > mLocalFileList[index].Timestamp)
                {
                    RedrawStatusText("正在更新..." + mServerFileList[i].Name);
                    if (!DownloadFile("http://" + mBucket + ".u.qiniudn.com/" + mServerFileList[i].Name,
                         curServeFileName, unixTime))
                        RedrawStatusText("更新失败..." + mServerFileList[i].Name);
                    mLocalFileList.RemoveAt(index);
                }
                else if (mServerFileList[i].Timestamp < mLocalFileList[index].Timestamp)
                {
                    RedrawStatusText("正在上传..." + mServerFileList[i].Name);
                    mIsDonePut = false;
                    PutFile(mServerFileList[i].Name, mSyncDir + "\\" + mServerFileList[i].Name, true);
                    WaitUntilTure(mIsDonePut);
                    if (mPutRet.OK)
                        // ReSharper disable once PossibleLossOfFraction
                        File.SetLastWriteTimeUtc(mSyncDir + "\\" + mServerFileList[i].Name,
                            new DateTime(1970, 1, 1).AddSeconds(mRsClient.Stat(new EntryPath(mBucket, mServerFileList[i].Name)).PutTime / 10000000));
                    else
                    {
                        CheckException(mPutRet.Exception.Message);
                        RedrawStatusText("同步失败");
                        mNotifyIcon.ShowBalloonTip(1000, "同步失败", mPutRet.Exception.Message, ToolTipIcon.Error);
                        return;
                    }
                    mLocalFileList.RemoveAt(index);
                }
                else if (mServerFileList[i].Timestamp == mLocalFileList[index].Timestamp)
                    mLocalFileList.RemoveAt(index);
                tmpCacheList.Add(curServeFileName);
            }

            #region 本地完全差异文件上传
            foreach (SyncFile sf in mLocalFileList)
            {
                // 存在缓存列表中说明已经上传过，因此属删除操作
                if (FindLocalCacheIndex(sf.Name) > -1)
                {
                    RedrawStatusText("正在删除（下行）..." + sf.Name);
                    File.Delete(mSyncDir + "\\" + sf.Name);
                    continue;
                }

                RedrawStatusText("正在上载..." + sf.Name);
                mIsDonePut = false;
                PutFile(sf.Name.Replace("\\", "/"), mSyncDir + "\\" + sf.Name);
                WaitUntilTure(mIsDonePut);
                if (mPutRet.OK)
                    // ReSharper disable once PossibleLossOfFraction
                    File.SetLastWriteTimeUtc(mSyncDir + "\\" + sf.Name,
                        new DateTime(1970, 1, 1).AddSeconds(
                            mRsClient.Stat(new EntryPath(mBucket, sf.Name.Replace("\\", "/"))).PutTime / 10000000));
                else
                {
                    CheckException(mPutRet.Exception.Message);
                    RedrawStatusText("上载失败");
                    mNotifyIcon.ShowBalloonTip(1000, "上载失败", mPutRet.Exception.Message, ToolTipIcon.Error);
                    return;
                }
                tmpCacheList.Add(sf.Name);
            }
            #endregion

            // 保存本地文件缓存
            StringBuilder sb = new StringBuilder();
            foreach (string s in tmpCacheList)
                sb.AppendLine(s);
            if (sb.Length > 0)
            {
                StreamWriter sw = new StreamWriter("Data/file_cache_list");
                sw.Write(sb.ToString());
                sw.Close();
            }

            RedrawStatusText("同步完成");
        }

        // 判断是否需要过滤
        private bool IsNeedFilter(string name)
        {
            if (mFilterList == null) return false;

            foreach (Filter f in mFilterList)
                switch (f.Type)
                {
                    case FilterType.Prefix:
                        if (name.StartsWith(f.Name))
                            return true;
                        break;
                    case FilterType.Suffix:
                        if (name.EndsWith(f.Name))
                            return true;
                        break;
                    case FilterType.FullMatch:
                        if (name.Equals(f.Name))
                            return true;
                        break;
                }

            return false;
        }

        // 加载本地文件（可用于递归操作）
        private void LoadLocalFiles(string dirPrefix)
        {
            DateTime timeStamp = new DateTime(1970, 1, 1);

            // 检查文件
            DirectoryInfo di = new DirectoryInfo(mSyncDir + dirPrefix);
            foreach (FileInfo fi in di.GetFiles())
            {
                string fileName = (dirPrefix.TrimStart('\\') + "\\" + fi.Name).TrimStart('\\').Replace("\\", "/");
                if (!IsNeedFilter(fileName))
                    mLocalFileList.Add(
                        new SyncFile(fileName, (fi.LastWriteTimeUtc.Ticks - timeStamp.Ticks) / 10000000));
                else
                    Console.WriteLine("过滤 " + fileName);
            }

            // 检查目录
            foreach (DirectoryInfo subDi in di.GetDirectories())
                LoadLocalFiles(dirPrefix + "\\" + subDi.Name);
        }

        // 查找修改文件列表中的匹配项
        private int FindChangeFileIndex(string name)
        {
            for (int i = 0; i < mChangeFileList.Count; i++)
                if (name.Equals(mChangeFileList[i].OldName) ||
                    name.Equals(mChangeFileList[i].NewName))
                    return i;
            return -1;
        }

        // 查找本地文件列表中的匹配项
        private int FindLocalFileIndex(string name)
        {
            for (int i = 0; i < mLocalFileList.Count; i++)
                if (mLocalFileList[i].Name.Equals(name))
                    return i;
            return -1;
        }

        // 查找本地缓存列表中的匹配项
        private int FindLocalCacheIndex(string name)
        {
            for (int i = 0; i < mLocalFileCache.Count; i++)
                if (mLocalFileCache[i].Equals(name))
                    return i;
            return -1;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="key">文件名称</param>
        /// <param name="fname">本地文件名称</param>
        /// <param name="rewrite">是否覆盖</param>
        private void PutFile(string key, string fname, bool rewrite = false)
        {
            string scope = mBucket;
            if (rewrite)
                scope += ":" + key;

            string upToken = new PutPolicy(scope).Token();
            IOClient client = new IOClient();
            client.PutFinished += (o, ret) =>
            {
                mPutRet = ret;
                mIsDonePut = true;
            };
            client.PutFile(upToken, key, fname, null);
        }

        // 循环等待
        private void WaitUntilTure(bool cond)
        {
            while (true)
            {
                if (cond)
                    return;
                Thread.Sleep(100);
            }
        }

        // 检查异常
        private void CheckException(string message)
        {
            mIsVaildKeys = !message.Contains("(401)");
            mIsVaildBucket = !message.Contains("(631)");
        }

        // 目录重命名事件
        private void DirectoryRenameEvent(string oldPath, string newPath)
        {
            DirectoryInfo di = new DirectoryInfo(newPath);
            foreach (FileInfo fi in di.GetFiles())
                FileRenameEvent((oldPath + "\\" + fi.Name).Substring(mSyncDir.Length + 1),
                    (newPath + "\\" + fi.Name).Substring(mSyncDir.Length + 1));

            foreach (DirectoryInfo subDi in di.GetDirectories())
                DirectoryRenameEvent(oldPath + "\\" + subDi.Name, subDi.FullName);
        }

        // 文件重命名事件
        private void FileRenameEvent(string oldPath, string newPath)
        {
            int index = FindChangeFileIndex(oldPath);
            if (index == -1)
                mChangeFileList.Add(new ChangeFile(oldPath, newPath));
            else
            {
                // 判断是否改回了原来的名字
                if (!mChangeFileList[index].OldName.Equals(newPath))
                    mChangeFileList.Add(new ChangeFile(mChangeFileList[index].OldName, newPath));
                mChangeFileList.RemoveAt(index);
            }
            Console.WriteLine("文件重命名：{0} {1}", oldPath, newPath);
        }

        // 下载文件
        private bool DownloadFile(string url, string key, DateTime unixTime)
        {
            // 判断是否为私有空间
            if (mIsPrivateBucket)
            {
                string baseUrl = GetPolicy.MakeBaseUrl(mBucket + ".u.qiniudn.com", key);
                url = GetPolicy.MakeRequest(baseUrl);
            }

            string path = mSyncDir + "\\" + key;
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            WebClient wb = new WebClient { Proxy = null };
            try
            {
                wb.DownloadFile(url, path);
                File.SetLastWriteTimeUtc(path, unixTime);
            }
            catch
            {
                return false;
            }
            return true;
        }

        #region 创建面板方法
        // 创建同步设置面板
        private void CreateSyncSettingPanel()
        {
            // 创建同步目录文本框
            CharmTextBox txtSyncDir = new CharmTextBox
            {
                Location = new Point(127 + MENU_WIDTH, 20 + TITLE_HEIGHT),
                Width = 260
            };
            // 创建浏览路径按钮
            CharmButton btnViewPath = new CharmButton
            {
                ButtonType = ButtonType.Classic_Size_08223,
                Text = "浏览路径",
                ForeColor = Color.DarkGreen,
                Location = new Point(399 + MENU_WIDTH, 22 + TITLE_HEIGHT)
            };

            // 创建同步周期文本框
            CharmTextBox txtSyncCycle = new CharmTextBox
            {
                Location = new Point(127 + MENU_WIDTH, 55 + TITLE_HEIGHT),
                Width = 50,
                TextInputMode = InputMode.Integer,
                MaxLength = 5,
                TextAlign = HorizontalAlignment.Center
            };

            // 创建空间名称文本框
            CharmTextBox txtBucket = new CharmTextBox
            {
                Location = new Point(290 + MENU_WIDTH, 55 + TITLE_HEIGHT),
                Width = 95,
            };
            // 创建私有空间检查框
            CharmCheckBox chkPrivateBucket = new CharmCheckBox
            {
                Location = new Point(400 + MENU_WIDTH, 57 + TITLE_HEIGHT),
                Text = "私有空间"
            };

            // 创建 AccessKey 文本框
            CharmTextBox txtAccessKey = new CharmTextBox
            {
                Location = new Point(127 + MENU_WIDTH, 125 + TITLE_HEIGHT),
                Width = 260,
                TextInputMode = InputMode.Password
            };
            // 创建查看 AccessKey 按钮
            CharmButton btnViewAccessKey = new CharmButton
            {
                ButtonType = ButtonType.Classic_Size_08223,
                Text = "查看密钥",
                ForeColor = Color.Red,
                Location = new Point(399 + MENU_WIDTH, 127 + TITLE_HEIGHT)
            };

            // 创建 SecretKey 文本框
            CharmTextBox txtSecretKey = new CharmTextBox
            {
                Location = new Point(127 + MENU_WIDTH, 160 + TITLE_HEIGHT),
                Width = 260,
                TextInputMode = InputMode.Password
            };
            // 创建查看 SecretKey 按钮
            CharmButton btnViewSecretKey = new CharmButton
            {
                ButtonType = ButtonType.Classic_Size_08223,
                Text = "查看密钥",
                ForeColor = Color.Red,
                Location = new Point(399 + MENU_WIDTH, 162 + TITLE_HEIGHT)
            };

            // 创建七牛开发平台链接标签
            CharmLinkLabel lblQiniuOpen = new CharmLinkLabel
            {
                Location = new Point(280, 230),
                ForeColor = Color.Blue,
                Text = "七牛云存储开发者平台"
            };

            // 关联控件事件
            txtSyncDir.TextChanged += txtSyncDir_TextChanged;
            btnViewPath.MouseClick += btnViewPath_MouseClick;
            txtSyncCycle.TextChanged += txtSyncDir_TextChanged;
            txtBucket.TextChanged += txtSyncDir_TextChanged;
            chkPrivateBucket.MouseClick += chkPrivateBucket_MouseClick;
            lblQiniuOpen.MouseClick += lblQiniuOpen_MouseClick;
            txtAccessKey.TextChanged += txtSyncDir_TextChanged;
            btnViewAccessKey.MouseClick += btnViewAccessKey_MouseClick;
            txtSecretKey.TextChanged += txtSyncDir_TextChanged;
            btnViewSecretKey.MouseClick += btnViewSecretKey_MouseClick;

            // 将控件添加到集合中
            this.Controls.Add(txtSyncDir);
            this.Controls.Add(txtSyncCycle);
            this.Controls.Add(txtAccessKey);
            this.Controls.Add(txtSecretKey);
            this.Controls.Add(txtBucket);

            // 创建同步设置面板控件集合
            mSyncSettingControls = new List<Control> { txtSyncDir, txtSyncCycle, txtAccessKey, txtSecretKey, txtBucket };
            mSyncSettingCharmControls = new List<CharmControl> { btnViewPath, lblQiniuOpen, btnViewAccessKey, btnViewSecretKey, chkPrivateBucket };
        }

        // 创建高级设置面板
        private void CreateAdvancedSettingPanel()
        {
            // 创建浏览路径按钮
            CharmButton btnReloadFilter = new CharmButton
            {
                ButtonType = ButtonType.Classic_Size_12425,
                Text = "重载过滤规则",
                ForeColor = Color.MediumPurple,
                Location = new Point(45 + MENU_WIDTH, 50 + TITLE_HEIGHT)

            };

            // 关联控件事件
            btnReloadFilter.MouseClick += btnReloadFilter_MouseClick;

            // 创建同步设置面板控件集合
            mAdvancedSettingCharmControls = new List<CharmControl> { btnReloadFilter };
        }

        // 创建关于面板
        private void CreateAboutPanel()
        {
            // 创建 GitHub 链接标签
            CharmLinkLabel lblGithub = new CharmLinkLabel
            {
                Location = new Point(185, 145),
                Font = new Font("微软雅黑", 10),
                ForeColor = Color.Blue,
                Text = "github.com/Unknwon/qiniudrive"
            };

            // 创建新浪微博链接标签
            CharmLinkLabel lblSina = new CharmLinkLabel
            {
                Location = new Point(330, 223),
                Font = new Font("微软雅黑", 10),
                ForeColor = Color.Blue,
                Text = "@无闻Unknown"
            };

            // 关联控件事件
            lblGithub.MouseClick += lblGithub_MouseClick;
            lblSina.MouseClick += lblSina_MouseClick;

            // 创建关于面板控件集合
            mAboutCharmControls = new List<CharmControl> { lblGithub, lblSina };
        }
        #endregion

        #region 绘制面板方法
        // 绘制同步设置面板
        private void DrawSyncSettingPanel(Graphics g)
        {
            // 同步目录
            g.DrawString("云盘同步目录：", this.Font, Brushes.Black, 125, 25 + TITLE_HEIGHT);
            // 同步周期
            g.DrawString("同步时间周期：", this.Font, Brushes.Black, 125, 60 + TITLE_HEIGHT);
            g.DrawString("秒", this.Font, Brushes.Black, 275, 60 + TITLE_HEIGHT);

            // 空间名称
            g.DrawString("空间名称：", this.Font, Brushes.Black, 310, 60 + TITLE_HEIGHT);

            // 密钥管理
            g.DrawString("密钥管理", this.Font, Brushes.Black, new Point(22 + MENU_WIDTH, 100 + TITLE_HEIGHT));
            g.DrawLine(Pens.DarkGray, 90 + MENU_WIDTH, 110 + TITLE_HEIGHT, this.Width - 50, 110 + TITLE_HEIGHT);
            g.DrawString("Access Key：", this.Font, Brushes.Black, 125, 130 + TITLE_HEIGHT);
            g.DrawString("Secret Key：", this.Font, Brushes.Black, 125, 165 + TITLE_HEIGHT);
            g.DrawString("申请注册七牛开发者帐号：", this.Font, Brushes.Black, 125, 200 + TITLE_HEIGHT);
        }

        // 绘制高级设置面板
        private void DrawAdvancedSettingPanel(Graphics g)
        {
            // 文件过滤
            g.DrawString("文件过滤", this.Font, Brushes.Black, new Point(22 + MENU_WIDTH, 22 + TITLE_HEIGHT));
            g.DrawLine(Pens.DarkGray, 90 + MENU_WIDTH, 32 + TITLE_HEIGHT, this.Width - 50, 32 + TITLE_HEIGHT);
        }

        // 绘制关于面板
        private static void DrawAboutPanel(Graphics g)
        {
            Font font = new Font("微软雅黑", 10, FontStyle.Bold);
            // 基本说明
            string intro = "本程序是基于七牛开放 API 构建的第三方同步程序\n" +
                                "七牛云存储官方不对本程序的任何行为负责。";
            g.DrawString(intro, font, Brushes.Black, 185, 60);
            // 开源说明
            intro = "七牛云盘由 C# 编写并已开源在 GitHub 上：";
            g.DrawString(intro, font, Brushes.Black, 185, 120);
            // 版权说明
            g.DrawImage(Properties.Resources.unknown, 190, 200, 64, 64);
            //font = new Font("微软雅黑", 10);
            intro = "程序作者：无闻\n" +
                    "新浪微博：\n" +
                    "个人博客：暂未开通";
            g.DrawString(intro, font, Brushes.Black, 265, 205);
            intro = "软件版本：" + APP_VER + "   版权所有 @ 2013 无闻";
            g.DrawString(intro, font, Brushes.Black, 205, 285);
            // 绘制七牛 LOGO
            g.DrawImage(Properties.Resources.qiniu_logo, 190, 330, 290, 45);
        }
        #endregion
        #endregion
    }
}

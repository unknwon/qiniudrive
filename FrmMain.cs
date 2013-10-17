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
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

using CharmCommonMethod;
using CharmControlLibrary;
#endregion

namespace QiNiuDrive
{
    // 七牛云盘主窗口
    public partial class FrmMain : Form
    {
        #region 常量
        private const string APP_NAME = "七牛云盘";             // 软件名称
        private const string APP_VER = "v0.0.1";                // 软件版本
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
        private bool mIsVaildSyncDir;               // 指示是否设置有效同步目录

        // * 用户控件 *
        private List<CharmControl> mCharmControls;          // Charm 控件集合
        private readonly ToolTip mToolTip = new ToolTip();  // 工具提示文本控件
        private NotifyIcon mNotifyIcon;                     // 托盘图标
        private CharmButton mBtnApply;                      // 应用按钮
        #endregion

        #region 同步设置
        // * 私有字段 *
        private bool mIsHasKeys;    // 指示是否填写密钥

        // * 用户控件 *
        private List<Control> mSyncSettingControls;             // 同步设置控件集合
        // 0-同步目录文本框；1-同步周期文本框；2-AccessKey 文本框；3-SecretKey 文本框
        private List<CharmControl> mSyncSettingCharmControls;   // 同步设置面板 Charm 控件集合
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
            //mDownloadSettingRedrawIndex = 0;
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
            {
                // 显示托盘菜单
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
            mBtnApply.Enabled = false;
            mIsLoadFinished = false;
            LoadLocalSetting();
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

            string keys = BasicMethod.DesEncrypt(((CharmTextBox)mSyncSettingControls[2]).Text, "QWERTYUI") + "|" +
                          BasicMethod.DesEncrypt(((CharmTextBox)mSyncSettingControls[3]).Text, "ASDFGHJK");
            StreamWriter sw = new StreamWriter("Config/KEY");
            sw.Write(keys);
            sw.Close();
            #endregion
        }
        #endregion

        #region 同步设置
        // 同步目录文本框文本改变事件（同步周期、AccessKey、SecretKey 文本框也关联此事件）
        private void txtSyncDir_TextChanged(object sender, EventArgs e)
        {
            // 首次启动时程序数据加载时不作响应
            if (mIsLoadFinished && !mBtnApply.Enabled)
            {
                mBtnApply.Enabled = true;
                this.Invalidate(mBtnApply.ClientRectangle); // 重绘控件
            }
        }

        // 浏览路径按钮被单击事件
        private void btnViewPath_MouseClick(object sender, MouseEventArgs e)
        {
            // 创建并实例化文件浏览对话框
            FolderBrowserDialog folderBrowserFialog = new FolderBrowserDialog();
            folderBrowserFialog.Description = "请选择下载文件保存目录";
            // 设置默认目录
            folderBrowserFialog.SelectedPath = ((CharmTextBox)mSyncSettingControls[0]).Text;

            // 显示对话框并判断用户是否指定新的目录
            if (folderBrowserFialog.ShowDialog() == DialogResult.OK)
                ((CharmTextBox)mSyncSettingControls[0]).Text = folderBrowserFialog.SelectedPath;    // 用户指定新的目录
        }

        // 七牛开发平台链接标签鼠标单击事件
        private static void lblQiniuOpen_MouseClick(object sender, MouseEventArgs e)
        {
            Process.Start("https://portal.qiniu.com/");
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

        #region 方法
        // 初始化设置
        private void InitializeSetting()
        {
            // 设置窗体属性
            this.Icon = Properties.Resources.icon;
            this.Text = APP_NAME;

            // 绘制窗体背景
            DrawFormBackground();

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
                mStatusText = "未启动同步";

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
            // 获取菜单项名称
            mMenuNames = IniOperation.ReadValue(APP_CFG_PATH, "setting", "menu_names").Split('|');

            #region 同步设置
            // 获取同步目录
            string syncDir = IniOperation.ReadValue(APP_CFG_PATH, "setting", "sync_dir");
            if (syncDir.Length > 0 && Directory.Exists(syncDir))
            {
                ((CharmTextBox)mSyncSettingControls[0]).Text = syncDir;
                mIsVaildSyncDir = true;
            }
            else
                mIsVaildSyncDir = false;

            // 获取同步周期
            int syncCycleNum;
            try
            {
                syncCycleNum = Convert.ToInt32(IniOperation.ReadValue(APP_CFG_PATH, "setting", "sync_cycle"));
            }
            catch
            {
                syncCycleNum = 60;
            }

            if (syncCycleNum < 60)
                syncCycleNum = 60;
            ((CharmTextBox)mSyncSettingControls[1]).Text = Convert.ToString(syncCycleNum);

            // 获取密钥
            if (File.Exists("Config/KEY"))
            {
                StreamReader sr = new StreamReader("Config/KEY");
                string[] keys = sr.ReadToEnd().Split('|');
                sr.Close();

                if (keys.Length > 0)
                {
                    ((CharmTextBox)mSyncSettingControls[2]).Text = BasicMethod.DesDecrypt(keys[0], "QWERTYUI");
                    ((CharmTextBox)mSyncSettingControls[3]).Text = BasicMethod.DesDecrypt(keys[1], "ASDFGHJK");
                    mIsHasKeys = true;
                }
            }
            #endregion

            mIsLoadFinished = true;
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

            // 创建同步设置面板控件集合
            mSyncSettingControls = new List<Control> { txtSyncDir, txtSyncCycle, txtAccessKey, txtSecretKey };
            mSyncSettingCharmControls = new List<CharmControl> { btnViewPath, lblQiniuOpen, btnViewAccessKey, btnViewSecretKey };
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
            g.DrawString("（单位：秒）", this.Font, Brushes.Black, 270, 60 + TITLE_HEIGHT);

            // 密钥管理
            g.DrawString("密钥管理", this.Font, Brushes.Black, new Point(22 + MENU_WIDTH, 100 + TITLE_HEIGHT));
            g.DrawLine(Pens.DarkGray, 90 + MENU_WIDTH, 110 + TITLE_HEIGHT, this.Width - 50, 110 + TITLE_HEIGHT);
            g.DrawString("Access Key：", this.Font, Brushes.Black, 125, 130 + TITLE_HEIGHT);
            g.DrawString("Secret Key：", this.Font, Brushes.Black, 125, 165 + TITLE_HEIGHT);
            g.DrawString("申请注册七牛开发者帐号：", this.Font, Brushes.Black, 125, 200 + TITLE_HEIGHT);
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
            intro = "版权所有 @ 2013 无闻";
            g.DrawString(intro, font, Brushes.Black, 265, 285);
            // 绘制七牛 LOGO
            g.DrawImage(Properties.Resources.qiniu_logo, 190, 330, 290, 45);
        }
        #endregion
        #endregion
    }
}

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
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

using CharmCommonMethod;
using CharmControlLibrary;
#endregion

namespace QiNiuDrive
{
    // 七牛云盘主窗口
    public partial class FrmMain : Form
    {
        #region 常量
        private const string APP_NAME = "七牛云盘";    // 软件名称
        private const string APP_VER = "v0.0.1";      // 软件版本
        #endregion

        #region 字段
        #region 主窗口相关
        // * 私有字段 *
        private string mStatusText = string.Empty;  // 底部状态文本

        // * 用户控件 *
        private List<CharmControl> mCharmControls;          // Charm 控件集合
        private readonly ToolTip mToolTip = new ToolTip();  // 工具提示文本控件
        private NotifyIcon mNotifyIcon;                     // 托盘图标
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
            //switch (mNavSelectedIndex)
            //{
            //    case 0: // 监控中心
            //        CharmControl.MouseClickEvent(e, MontinorCenterCharmControls);
            //        break;
            //}
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
                #region  重绘导航栏
                // 判断鼠标是否在合适纵选区内
                //if (e.Y > 26 && e.Y < 101)
                //{
                //    // 判断鼠标是否在合适横选区内
                //    if (e.X < (15 + MAX_NAV_INDEX * 78 + 56))
                //    {
                //        // 判断鼠标是否在当前选区内
                //        for (int i = 0; i <= MAX_NAV_INDEX; i++)
                //        {
                //            if (mNavSelectedIndex != i && e.X > (2 + i * 78) && e.X < (10 + i * 78 + 66))
                //            {
                //                mNavSelectedIndex = i;  // 设置菜单栏现行选中项索引
                //                ShowPanels();   // 设置控件可见性
                //                this.Invalidate();
                //                return; // 不需要再继续进行检查
                //            }
                //        }
                //    }
                //}
                #endregion

                // 过程中需要用到的变量
                int offsetY = 107;

                // 根据菜单现行选中项索引判断绘制哪个面板
                //switch (mNavSelectedIndex)
                //{
                //    case 0: // 监控中心
                //        #region 监控中心面板

                //        #endregion
                //        break;
                //}
            }

            // 指示是否捕捉到事件
            bool isCaptureEvent = false;

            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            //switch (mNavSelectedIndex)
            //{
            //    case 0: // 监控中心
            //        isCaptureEvent = CharmControl.MouseDownEvent(e, MontinorCenterCharmControls, this);
            //        break;
            //}
            #endregion

            // 判断之前是否已捕捉到事件
            if (!isCaptureEvent)
            {
                // 调用事件
                if (!CharmControl.MouseDownEvent(e, mCharmControls, this))
                    APIOperation.MoveNoBorderForm(this, e);
            }
        }

        // 窗体鼠标移动事件
        private void frmMain_MouseMove(object sender, MouseEventArgs e)
        {
            // 过程需要用到的变量
            List<Rectangle> rectRedrawList = new List<Rectangle>(); // 重绘区域集合
            bool isChange = false; // 指示状态是否变化

            #region  重绘导航栏
            // 判断鼠标是否在合适纵选区内
            //if (e.Y > 26 && e.Y < 101)
            //{
            //    // 判断鼠标是否在合适横选区内
            //    for (int i = 0; i <= MAX_NAV_INDEX; i++)
            //    {
            //        // 判断鼠标是否在当前选区内
            //        if (e.X > (2 + i * 78) && e.X < (10 + i * 78 + 66))
            //        {
            //            isChange = true;
            //            if (mNavHighlightIndex != i)
            //            {
            //                mNavHighlightIndex = i;
            //                rectRedrawList.Add(new Rectangle(new Point(3, 25), new Size((MAX_NAV_INDEX + 1) * 78, 76)));
            //            }
            //            break;
            //        }
            //    }

            //    if (!isChange)
            //    {
            //        if (mNavHighlightIndex > -1)
            //        {
            //            mNavHighlightIndex = -2;
            //            rectRedrawList.Add(new Rectangle(new Point(3, 25), new Size((MAX_NAV_INDEX + 1) * 78, 76)));
            //        }
            //    }
            //}
            //else
            //{
            //    if (mNavHighlightIndex > -1)
            //    {
            //        mNavHighlightIndex = -2;
            //        rectRedrawList.Add(new Rectangle(new Point(3, 25), new Size((MAX_NAV_INDEX + 1) * 78, 76)));
            //    }
            //}
            #endregion

            // 过程中需要用到的变量
            int offsetY = 107;
            isChange = false;

            // 根据菜单现行选中项索引判断绘制哪个面板
            //switch (mNavSelectedIndex)
            //{
            //    case 0: // 监控中心
            //        #region 监控中心面板

            //        #endregion
            //        break;
            //}

            //// 判断是否存在需要重绘的控件
            //if (rectRedrawList.Count > 0)
            //{
            //    Rectangle rectRedraw = rectRedrawList[0];
            //    for (int i = 1; i < rectRedrawList.Count; i++)
            //        rectRedraw = Rectangle.Union(rectRedraw, rectRedrawList[i]);
            //    this.Invalidate(rectRedraw);
            //}

            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            //switch (mNavSelectedIndex)
            //{
            //    case 0: // 监控中心
            //        CharmControl.MouseMoveEvent(e, MontinorCenterCharmControls, this, mToolTip);
            //        break;
            //}
            #endregion

            // 调用事件
            CharmControl.MouseMoveEvent(e, mCharmControls, this, mToolTip);
        }

        // 窗体鼠标弹起事件
        private void frmMain_MouseUp(object sender, MouseEventArgs e)
        {
            // 只响应鼠标左键
            //if (e.Button == MouseButtons.Left)
            //{
            //    // 过程中需要用到的变量
            //    int offsetY = 107;
            //    bool isChange = false;

            //    // 根据菜单现行选中项索引判断绘制哪个面板
            //    switch (mNavSelectedIndex)
            //    {
            //        case 0: // 监控中心
            //            #region 监控中心面板

            //            #endregion
            //            break;
            //    }
            //}

            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            //switch (mNavSelectedIndex)
            //{
            //    case 0: // 监控中心
            //        CharmControl.MouseUpEvent(e, MontinorCenterCharmControls, this);
            //        break;
            //}
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

            #region 绘制菜单栏
            // 轮询菜单项
            //for (int i = 0; i < mMenuNames.Length; i++)
            //{
            //    // 判断是否为现行选中项
            //    if (mMenuSelectedIndex == i)
            //    {
            //        g.DrawImage(Properties.Resources.tab_button_setting_chosen, new Point(1, 23 + i * 26));
            //        g.DrawString(mMenuNames[i], this.Font, Brushes.White, new Point(20, 28 + i * 26));
            //    }
            //    else
            //        g.DrawString(mMenuNames[i], this.Font, Brushes.Black, new Point(20, 28 + i * 26));
            //}
            #endregion

            #region 绘制主面板
            // 根据菜单现行选中项索引判断绘制哪个面板
            //switch (mMenuSelectedIndex)
            //{
            //    case 0: // 综合设置
            //        DrawMultipleSettingPanel(g);
            //        CharmControl.PaintEvent(e.Graphics, MultipleSettingCharmControls);
            //        break;
            //    case 1: // 下载设置
            //        DrawDownloadSettingPanel(g);
            //        CharmControl.PaintEvent(e.Graphics, DownloadSettingCharmControls);
            //        break;
            //}
            #endregion

            // 绘制控件
            CharmControl.PaintEvent(e.Graphics, mCharmControls);

            // 重置重绘索引
            //mMultipleSettingRedrawIndex = 0;
            //mDownloadSettingRedrawIndex = 0;
        }
        #endregion

        #region 控件事件
        // 关闭按钮被单击事件
        private void btnClose_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
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

            #region 创建窗体组件
            // 创建关闭系统按钮
            CharmSysButton btnClose = new CharmSysButton
            {
                SysButtonType = SysButtonType.Close,
                ToolTipText = "关闭",
                Location = new Point(this.Width - 44, 1)
            };

            // 创建控件集合
            mCharmControls = new List<CharmControl> { btnClose };

            // 关联控件事件
            btnClose.MouseClick += btnClose_MouseClick;
            #endregion

            #region 创建托盘图标
            // 创建托盘图标
            mNotifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.icon,
                Text = APP_NAME + "\n" + "未启动同步",
                Visible = true
            };

            // * 关联控件事件 *
            //mNotifyIcon.MouseClick += new MouseEventHandler(notifyIcon_MouseClick);
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
                const int titleHeight = 30;
                const int bottomHeight = 45;
                const int menuWidth = 90;

                // 绘制标题栏背景
                SolidBrush sb = new SolidBrush(Color.DodgerBlue);
                g.FillRectangle(sb, 0, 0, this.Width, titleHeight);
                // 绘制窗口图标
                g.DrawImage(Properties.Resources.logo, new Rectangle(6, 5, 20, 20));
                // 绘制窗口标题
                g.DrawString("设置中心 - 七牛云盘",
                    new Font("微软雅黑", 10, FontStyle.Bold), Brushes.White, new Point(30, 6));
                // 绘制竖线
                g.DrawLine(Pens.DarkGray, menuWidth + 1, titleHeight, 91, this.Height);
                // 绘制菜单区
                sb.Color = Color.FromArgb(100, Color.LightGray);
                g.FillRectangle(sb, 1, titleHeight, 90, this.Height);
                // 绘制主面板区
                sb.Color = Color.FromArgb(255, Color.WhiteSmoke);
                g.FillRectangle(sb, menuWidth + 2, titleHeight, this.Width - menuWidth - 4, this.Height - titleHeight - bottomHeight);
                // 绘制横线
                g.DrawLine(Pens.DarkGray, menuWidth + 2, this.Height - bottomHeight, this.Width - 2, this.Height - bottomHeight);
                //// 绘制按钮区
                sb.Color = Color.FromArgb(70, Color.LightGray);
                g.FillRectangle(sb, menuWidth + 2, this.Height - bottomHeight, this.Width - 2, bottomHeight);
                // 绘制边框
                g.DrawRectangle(new Pen(Color.DimGray), 0, 0, this.Width - 1, this.Height - 1);
            }
            #endregion

            // 设置背景资源
            this.BackgroundImage = new Bitmap(imgBkg);

            // 释放系统资源
            imgBkg.Dispose();
        }
        #endregion
    }
}

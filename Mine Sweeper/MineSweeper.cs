using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mine_Sweeper
{
    public partial class MineSweeper : Form
    {
        public MineSweeper()
        {
            InitializeComponent();
        }

        //图像
        Graphics myg;
        
        //设置雷属性
        int[] mine;//雷编号
        int minenum, minerec;//雷数量；被标记的雷数量
        
        //图尺寸
        int hgtnum, widnum, hgt, wid;//行数；列数；单块纵像素；单块横像素
        
        //当前块是否被点击过（0：未被点击；1：已被点击）
        int[] isClicked;
        
        //当前块是否被标记为雷（0：标记为雷；1：未标记为雷）
        int[] rec;

        //计时器
        public int currentcount = 0;

        //图片素材
        public static Image img_mine = System.Drawing.Image.FromFile(Application.StartupPath + "/mine.jpg");
        public static Image img_tag = System.Drawing.Image.FromFile(Application.StartupPath + "/tag.jpg");
        public static Image img_bg = System.Drawing.Image.FromFile(Application.StartupPath + "/bg.png");
        //小尺寸（normal hard）
        Bitmap smallmine = new Bitmap(img_mine, 34, 34);
        Bitmap smalltag = new Bitmap(img_tag, 34, 34);
        Bitmap smallbg = new Bitmap(img_bg, 34, 34);
        //大尺寸（easy）
        Bitmap bigmine = new Bitmap(img_mine, 62, 62);
        Bitmap bigtag = new Bitmap(img_tag, 62, 62);
        Bitmap bigbg = new Bitmap(img_bg, 62, 62);

        //素材尺寸选择 (0：小尺寸(normal hard);1：大尺寸(easy))
        int size;


        /// <summary>
        /// 绘制背景
        /// </summary>
        /// <param name="g">图</param>
        /// <param name="size">素材尺寸选择</param>
        public void drawbackgroud(Graphics g, int size)
        {
            //非图像绘制
            {
                //Point[] top = new Point[widnum + 1];
                //for (int i = 0; i < top.Length; i++)
                //{
                //    top[i] = new Point(wid * i, 0);
                //}
                //Point[] border = new Point[widnum + 1];
                //for (int i = 0; i < border.Length; i++)
                //{
                //    border[i] = new Point(wid * i, hgt * hgtnum);
                //}
                //Point[] lft = new Point[hgtnum + 1];
                //for (int i = 0; i < lft.Length; i++)
                //{
                //    lft[i] = new Point(0, hgt * i);
                //}
                //Point[] rgt = new Point[hgtnum + 1];
                //for (int i = 0; i < rgt.Length; i++)
                //{
                //    rgt[i] = new Point(wid * widnum, hgt * i);
                //}

                //for (int i = 0; i < widnum + 1; i++)
                //{
                //    g.DrawLine(new Pen(new SolidBrush(Color.Black), 2), top[i], border[i]);
                //}
                //for (int i = 0; i < hgtnum + 1; i++)
                //{
                //    g.DrawLine(new Pen(new SolidBrush(Color.Black), 2), lft[i], rgt[i]);
                //}
            }

            //图像绘制
            for (int i = 0; i < widnum * hgtnum + 1; i++)
            {
                //获取单块坐标，左上点位像素坐标
                Point p = getLoc(i);
                int[] loc = { p.X, p.Y };
                Point lft_top = new Point((loc[0] - 1) * wid + 2, (loc[1] - 1) * hgt + 2);
                //绘制
                if (size == 0)//小尺寸
                {
                    g.DrawImage(smallbg, lft_top);
                }
                else//大尺寸
                {
                    g.DrawImage(bigbg, lft_top);
                }
            }
        }

        /// <summary>
        /// 随机生成雷（索引）
        /// </summary>
        /// <param name="num">雷数量</param>
        /// <returns>雷索引</returns>
        public int[] getMine(int num)
        {
            Random ran = new Random();
            int[] mine = new int[num];
            for (int i = 0; i < num; i++)
            {
                mine[i] = ran.Next(1, hgtnum * widnum + 1);//随机生成雷
                while (contains(mine[i], mine, i))//判断索引是否重复
                {
                    mine[i] = ran.Next(1, hgtnum * widnum + 1);
                }
            }
            return mine;
        }
        
        /// <summary>
        /// 数组前若干元素是否含有目标值（用于雷索引去重）
        /// </summary>
        /// <param name="tag">目标值</param>
        /// <param name="a">数组</param>
        /// <param name="endindex">查询截止位置</param>
        /// <returns>是/否</returns>
        public bool contains(int tag, int[] a, int endindex)
        {
            for (int i = 0; i < endindex - 1; i++)
            {
                if (a[i] == tag) return true;
            }
            return false;
        }
        
        /// <summary>
        /// 根据索引获取块坐标
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>块坐标</returns>
        public Point getLoc(int index)
        {
            Point loc;
            if (index % widnum == 0)
                loc = new Point(widnum, index / widnum);
            else
                loc = new Point(index % widnum, index / widnum + 1);
            return loc;
        }
        
        /// <summary>
        /// 获取目标块临块索引
        /// </summary>
        /// <param name="index">目标块</param>
        /// <returns>临块</returns>
        public int[] getAround(int index)
        {
            int[] around;
            //四角位
            if (index == 1 || index == widnum || index == 1 + widnum * (hgtnum - 1) || index == widnum * hgtnum)
            {
                around = new int[3];
                if (index == 1)
                {
                    around[0] = index + 1;
                    around[1] = index + widnum;
                    around[2] = index + widnum + 1;
                }
                else if (index == widnum)
                {
                    around[0] = index - 1;
                    around[1] = index + widnum;
                    around[2] = index + widnum - 1;
                }
                else if (index == 1 + widnum * (hgtnum - 1))
                {
                    around[0] = index + 1;
                    around[1] = index - widnum;
                    around[2] = index - widnum + 1;
                }
                else
                {
                    around[0] = index - 1;
                    around[1] = index - widnum;
                    around[2] = index - widnum - 1;
                }
            }
            //边位
            else if (index > 1 && index < widnum || index > (1 + widnum * (hgtnum - 1)) && index < (widnum * hgtnum) || index % widnum == 1 || index % widnum == 0)
            {
                around = new int[5];
                if (index > 1 && index < widnum)
                {
                    around[0] = index - 1;
                    around[1] = index + 1;
                    around[2] = index + widnum - 1;
                    around[3] = index + widnum;
                    around[4] = index + widnum + 1;
                }
                else if (index > (1 + widnum * (hgtnum - 1)) && index < (widnum * hgtnum))
                {
                    around[0] = index - 1;
                    around[1] = index + 1;
                    around[2] = index - widnum - 1;
                    around[3] = index - widnum;
                    around[4] = index - widnum + 1;
                }
                else if (index % widnum == 1)
                {
                    around[0] = index - widnum;
                    around[1] = index + 1;
                    around[2] = index - widnum + 1;
                    around[3] = index + widnum;
                    around[4] = index + widnum + 1;
                }
                else
                {
                    around[0] = index - widnum;
                    around[1] = index - 1;
                    around[2] = index - widnum - 1;
                    around[3] = index + widnum;
                    around[4] = index + widnum - 1;
                }
            }
            //常规位
            else
            {
                around = new int[8];
                around[0] = index - widnum - 1;
                around[1] = around[0] + 1;
                around[2] = around[1] + 1;
                around[3] = index - 1;
                around[4] = index + 1;
                around[5] = index + widnum - 1;
                around[6] = around[5] + 1;
                around[7] = around[6] + 1;
            }
            return around;
        }
        
        /// <summary>
        /// 判断目标块是否为雷
        /// </summary>
        /// <param name="index">目标块</param>
        /// <param name="mine">雷索引</param>
        /// <returns>是/否</returns>
        public bool isMine(int index, int[] mine)
        {
            for (int i = 0; i < mine.Length; i++)
            {
                if (index == mine[i]) return true;
            }
            return false;
        }
        
        /// <summary>
        /// 获取目标块临块雷数目
        /// </summary>
        /// <param name="index">目标块</param>
        /// <returns>类数目</returns>
        public int getTag(int index)
        {
            int minenum = 0;
            int[] around = getAround(index);
            for (int i = 0; i < around.Length; i++)
            {
                if (isMine(around[i], mine)) minenum++;
            }
            return minenum;
        }
        
        /// <summary>
        /// 左键点击目标块事件
        /// </summary>
        /// <param name="g">图</param>
        /// <param name="index">目标块</param>
        /// <param name="tag">临块雷数目</param>
        /// <param name="size">素材尺寸</param>
        public void click_draw(Graphics g, int index, int tag, int size)
        {
            if (isClicked[index - 1] == 0)//未被点击过
            {
                //获取左上像素坐标
                Point p = getLoc(index);
                int[] loc = { p.X, p.Y };
                Point lft_top = new Point((loc[0] - 1) * wid, (loc[1] - 1) * hgt);
                
                //绘制雷
                //Point rgt_border = new Point((index[0]) * wid, (index[1]) * hgt);
                if (tag == -1)
                {
                    //g.FillEllipse(new SolidBrush(Color.DarkRed), lft_top.X + 2, lft_top.Y + 2, wid - 4, hgt - 4);
                    if (size == 0)
                    {
                        g.DrawImage(smallmine, lft_top.X + 2, lft_top.Y + 2);
                    }
                    else g.DrawImage(bigmine, lft_top.X + 2, lft_top.Y + 2);
                    isClicked[index - 1] = 1;//更新被点击状态
                }
                //绘制空白位
                else if (tag == 0)
                {
                    g.FillRectangle(new SolidBrush(Color.White), lft_top.X + 2, lft_top.Y + 2, wid - 2, hgt - 2);
                    isClicked[index - 1] = 1;//更新被点击状态
                    
                    //空白位临块为非雷位，默认被点击
                    int[] around = getAround(index);
                    for (int i = 0; i < around.Length; i++)
                    {
                        click_draw(g, around[i], getTag(around[i]), size);
                    }
                }
                //绘制数字位
                else
                {
                    g.FillRectangle(new SolidBrush(Color.White), lft_top.X + 2, lft_top.Y + 2, wid - 2, hgt - 2);
                    if (size == 0) g.DrawString(tag.ToString(), new Font("华文隶书", 30), new SolidBrush(Color.Green), lft_top);
                    else g.DrawString(tag.ToString(), new Font("华文隶书", 50), new SolidBrush(Color.Green), lft_top);
                    isClicked[index - 1] = 1;//更新被点击状态
                }
            }
        }
        
        /// <summary>
        /// 判断是否找出了所有雷（点击了所有非雷位视为胜利）
        /// </summary>
        /// <returns>是/否</returns>
        public bool isWin()
        {
            int sum = 0;
            for (int i = 0; i < isClicked.Length; i++)
            {
                sum += isClicked[i];
            }
            if (sum == widnum * hgtnum - mine.Length) return true;
            else return false;
        }
        
        /// <summary>
        /// 获取已用时间并生成特定格式 00:00
        /// </summary>
        /// <param name="t">时间戳</param>
        /// <returns>已用时间</returns>
        public string getTime(int t)
        {
            string time = "";
            int min, sec;
            sec = t % 60;
            min = t / 60;
            string m, s;
            if (min < 10) m = "0" + min.ToString();
            else m = min.ToString();
            if (sec < 10) s = "0" + sec.ToString();
            else s = sec.ToString();
            time = m + ":" + s;
            return time;
        }

        /// <summary>
        /// 设置背景尺寸
        /// </summary>
        /// <param name="tag">0：大尺寸；1：小尺寸</param>
        public void set_bgSize(int tag)
        {
            if (tag == 0)
            {
                this.Width = 598;
                this.Height = 676;
                panel1.Height = 582;
                button1.Width = panel3.Width / 3 - 50;
                button2.Width = panel3.Width / 3 - 50;
                button3.Width = panel3.Width / 3 - 50;
                pictureBox1.Location = new Point(button1.Width * 3 + 15, 5);
                label1.Location = new Point(pictureBox1.Location.X + pictureBox1.Width + 2, 5);
                label2.Location = new Point(pictureBox1.Location.X, 5 + pictureBox1.Width);
            }
            else
            {
                this.Width = 1102;
                this.Height = 676;
                panel1.Height = 582;
                button1.Width = panel3.Width / 3 - 50;
                button2.Width = panel3.Width / 3 - 50;
                button3.Width = panel3.Width / 3 - 50;
                pictureBox1.Location = new Point(button1.Width * 3 + 15, 5);
                label1.Location = new Point(pictureBox1.Location.X + pictureBox1.Width + 2, 5);
                label2.Location = new Point(pictureBox1.Location.X, 5 + pictureBox1.Width);
            }
        }

        /// <summary>
        /// 模式设置
        /// </summary>
        /// <param name="bgSize">背景尺寸</param>
        /// <param name="picsize">素材尺寸</param>
        /// <param name="w">列数</param>
        /// <param name="h">行数</param>
        /// <param name="m_num">雷数量</param>
        public void ModeSet(int bgSize,int picsize,int w,int h,int m_num)
        {
            //设置背景尺寸
            set_bgSize(bgSize);
            //开启图区权限
            pictureBoxMine.Enabled = true;
            //设置素材尺寸
            size = picsize;
            //设置图尺寸
            widnum = w;
            hgtnum = h;
            wid = pictureBoxMine.Width / widnum;
            hgt = pictureBoxMine.Height / hgtnum;

            //绘制背景
            myg = pictureBoxMine.CreateGraphics();
            myg.Clear(Color.Black);
            drawbackgroud(myg, size);

            //生成雷
            minenum = m_num;
            minerec = minenum;
            label1.Text = "Mine : " + minerec;
            mine = getMine(minenum);

            //初始化被点击状态
            isClicked = new int[widnum * hgtnum];
            for (int i = 0; i < isClicked.Length; i++)
            {
                isClicked[i] = 0;
            }

            //初始化被标记状态
            rec = new int[widnum * hgtnum];
            for (int i = 0; i < rec.Length; i++)
            {
                rec[i] = 0;
            }

            //重置时间戳，开始计时
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Start();
            currentcount = 0;
        }
        
        //easy mode
        private void button1_Click(object sender, EventArgs e)
        {
            ModeSet(0, 1, 9, 9, 10);
        }
        //normal mode
        private void button2_Click(object sender, EventArgs e)
        {
            ModeSet(0, 0, 16, 16, 40);
        }
        //hard mode
        private void button3_Click(object sender, EventArgs e)
        {
            ModeSet(1, 0, 30, 16, 99);
        }
        //导入事件
        private void MineSweeper_Load(object sender, EventArgs e)
        {
            //关闭图区权限
            pictureBoxMine.Enabled = false;
            //初始化小尺寸（背景）
            set_bgSize(0);
        }
        //鼠标事件
        private void pictureBoxMine_MouseClick(object sender, MouseEventArgs e)
        {
            //根据单击位置获得块索引
            Point p = e.Location;
            int x = p.X;
            int y = p.Y;
            int index = (x / wid) + (y / hgt) * widnum + 1;

            //[未胜利时]右键标记雷（目标块未被左键点击过）
            if (!isWin() && e.Button == MouseButtons.Right && isClicked[index - 1] == 0)
            {
                if (rec[index - 1] == 0)//未标记
                {
                    //获取左上像素坐标
                    Point pt = getLoc(index);
                    int[] loc = { pt.X, pt.Y };
                    Point lft_top = new Point((loc[0] - 1) * wid + 2, (loc[1] - 1) * hgt + 2);

                    //绘制标记
                    //myg.FillRectangle(new SolidBrush(Color.Yellow), lft_top.X + 2, lft_top.Y + 2, wid - 2, hgt - 2);
                    if (size == 0)
                    {
                        myg.DrawImage(smalltag, lft_top);
                    }
                    else myg.DrawImage(bigtag, lft_top);

                    //更新标记状态
                    rec[index - 1] = 1;
                    minerec -= 1;
                    label1.Text = "Mine : " + minerec;
                }
                else//已被标记，移除标记
                {
                    Point pt = getLoc(index);
                    int[] loc = { pt.X, pt.Y };
                    Point lft_top = new Point((loc[0] - 1) * wid + 2, (loc[1] - 1) * hgt + 2);

                    //myg.FillRectangle(new SolidBrush(Color.Gray), lft_top.X + 2, lft_top.Y + 2, wid - 2, hgt - 2);
                    if (size == 0)
                    {
                        myg.DrawImage(smallbg, lft_top);
                    }
                    else myg.DrawImage(bigbg, lft_top);

                    rec[index - 1] = 0;
                    minerec += 1;
                    label1.Text = "Mine : " + minerec;
                }
            }
            //左键点击事件
            else
            {
                if (!isWin())//未胜利
                {
                    if (rec[index - 1] == 1)//被标记的块被点击则移除标记
                    {
                        minerec += 1;
                        label1.Text = "Mine : " + minerec;
                    }
                    if (isMine(index, mine))//点击雷位，显示所有雷位，游戏结束（停止计时，图区不可再点击）
                    {
                        click_draw(myg, index, -1, size);
                        for (int i = 0; i < mine.Length; i++)
                        {
                            System.Threading.Thread.Sleep(20);
                            click_draw(myg, mine[i], -1, size);
                        }

                        this.timer1.Stop();
                        MessageBox.Show("You Lose!");

                        for (int i = 0; i < isClicked.Length; i++)
                        {
                            isClicked[i] = 1;
                        }
                    }
                    else//点击非雷位
                        click_draw(myg, index, getTag(index), size);

                    //胜利，停止计时，图区不可再点击
                    if (isWin()) { this.timer1.Stop(); MessageBox.Show("Congratulations！You Win！"); }
                }
            }
        }
        //时间戳计时
        private void timer1_Tick(object sender, EventArgs e)
        {
            currentcount += 1;
            string t = currentcount.ToString().Trim();
            this.label2.Text = "Time : " + getTime(Convert.ToInt32(t));
        }
    }
}
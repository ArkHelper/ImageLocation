using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging; //nuget //System.Drawing.Common
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLocation
{
    internal class Program
    {
        /// <summary>
        /// 原图地址
        /// </summary>
        static string bigpic, smallpic;

        /// <summary>
        /// 点容差（0~1）
        /// </summary>
        static readonly double errorCon = 0.7;
        /// <summary>
        /// 色容差（0~255）
        /// </summary>
        static readonly int errorRange = 10;

        /// <summary>
        /// 精确度（取点数量）
        /// </summary>
        static readonly int num = 1000;

        static void Main(string[] args)
        {
            //确定文件地址
            Console.WriteLine("大图片地址：");
            bigpic = Console.ReadLine();
            Console.WriteLine("小图片地址：");
            smallpic = Console.ReadLine();

            //初始化图像类
            var bigBM = new Bitmap(bigpic);
            var smallBM = new Bitmap(smallpic);

            //声明数组，写入随机坐标
            Point[] pointSmall = new Point[num];
            Random rand = new Random();
            for (int i = 0; i < num; i++)
            {
            repeat:;
                Point aa = new Point(rand.Next(0, smallBM.Width), rand.Next(0, smallBM.Height));
                if (pointSmall.Contains(aa)) { goto repeat; }
                else { pointSmall[i] = aa; }
            }

            Point[] pointBig = new Point[num]; //声明数组，稍后写入以上随机坐标在大图中的对应坐标
            Point[] pointDistance = new Point[num]; //反映其他点与第一点的位置关系
            Color[] pointColor = new Color[num]; //获取点的颜色
            for (int i = 0; i < num; i++)
            {
                pointBig[i] = new Point(0, 0);
                pointDistance[i] = PointSub(pointSmall[0], pointSmall[i]);
                pointColor[i] = smallBM.GetPixel(pointSmall[i].X, pointSmall[i].Y);
            }

            // 声明数组，用于写入pointBig[]
            ArrayList pointBigArray = new ArrayList();

            //在大图中检索第一个点
            for (int y = 0; y < bigBM.Height; y++)
            {
                for (int x = 0; x < bigBM.Width; x++)
                {
                    //第一点颜色匹配
                    if (IsColor(bigBM.GetPixel(x, y), pointColor[0]))
                    {
                        //测试第一点
                        pointBig[0] = new Point(x, y);

                        int a = 0; //容差计算1
                        for (int i = 1; i < num; i++)
                        {
                            //算出对应点的坐标
                            pointBig[i] = PointSub(pointBig[0], pointDistance[i]);

                            //跳过错误点
                            //我也不知道用不用加 但是我不想思考了
                            if (pointBig[i].X < 0
                                || pointBig[i].Y < 0
                                || pointBig[i].X >= bigBM.Width
                                || pointBig[i].Y >= bigBM.Height)
                            { break; }

                            //容差计算2 推测出其他点 判断对应颜色是否符合
                            if (IsColor(bigBM.GetPixel(pointBig[i].X, pointBig[i].Y), pointColor[i])) { a += 1; }
                        }
                        if (a >= (num * errorCon))
                        {
                            //必须强制手动转换一次（以下）
                            //否则ArrayList会直接从内存中寻址
                            //造成数据重复
                            Point[] _points = new Point[num];
                            for (int i = 0; i < num; i++)
                            {
                                _points[i] = pointBig[i];
                            }
                            pointBigArray.Add(_points);
                        }
                    }
                }
            }

            //遍历全部结束

            //结束 语句
            if (pointBigArray.Count > 0)
            //存在图片 语句
            {
                Console.WriteLine(@"结果保存位置（地址:\文件名.扩展名）：");
                string result = Console.ReadLine();
                Graphics grapPic = Graphics.FromImage(bigBM);
                foreach (Point[] pointBigInList in pointBigArray)
                {
                    //定位小图(0,0)点在大图中的位置
                    Point leftTopPointSmall = PointSub(pointBigInList[0], pointSmall[0]);
                    //画矩形
                    Rectangle areaRect = new Rectangle(leftTopPointSmall.X, leftTopPointSmall.Y, smallBM.Width, smallBM.Height);
                    grapPic.DrawRectangle(new Pen(Color.Red, 3), areaRect);
                }
                bigBM.Save(result);
            }
            //不存在图片 语句
            else
            {
                Console.WriteLine("未检出该图，请降低容差数值重试");
            }
            //结束 语句
            Console.WriteLine("按任意键继续");
            Console.ReadKey();
        }

        /// <summary>
        /// 坐标加和
        /// </summary>
        /// <param name="points">Point数组</param>
        /// <returns>Point(横坐标相加，纵坐标相加)</returns>
        internal static Point PointPlus(params Point[] points)
        {
            Point point = new Point(0, 0);
            for (int i = 0; i < points.Length; i++)
            {
                point.X += points[i].X;
                point.Y += points[i].Y;
            }
            return point;
        }

        /// <summary>
        /// 坐标相减
        /// </summary>
        /// <param name="point1">被减数</param>
        /// <param name="point2">减数</param>
        /// <returns>Point相减结果</returns>
        internal static Point PointSub(Point point1, Point point2)
        {
            Point point = new Point(point1.X - point2.X, point1.Y - point2.Y);
            return point;
        }
        /// <summary>
        /// 色差计算
        /// </summary>
        internal static bool IsColor(Color colorA, Color colorB)
        {
            return colorA.A <= colorB.A + errorRange && colorA.A >= colorB.A - errorRange &&
                   colorA.R <= colorB.R + errorRange && colorA.R >= colorB.R - errorRange &&
                   colorA.G <= colorB.G + errorRange && colorA.G >= colorB.G - errorRange &&
                   colorA.B <= colorB.B + errorRange && colorA.B >= colorB.B - errorRange;
        }
    }
}

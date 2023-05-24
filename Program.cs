using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Coil_Batching_TS
{
    //最简化，也就是只考虑ij在一起的收益以及成本，
    //没有考虑不匹配的情况，而且假设的是所有的都可以退火的情况

    internal class Program
    {

        static void Main(string[] args)
        {
            //Coil demo = new Coil();
            TS_Solver solver = new TS_Solver();
            solver.solver();

        }
    }

    public class Coil
    {
        static int GetRandomSeed()
        {
            byte[] bytes = new byte[100];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        Random random = new Random(GetRandomSeed());
        static int Max_Weight = 10;
        static int Max_Height = 5;
        static int Max_PRI = 10;
        private static int weight;
        private static int height;
        private static int PRI;
        static int match_number_c;//最多智能有总数减1个可以和它一起
        static int match_number_f;
        //public int[] match_coil;//与它本身可以匹配的coil
        //public int[] match_furnace;//与该coil可以匹配的furnace

        //public List<int> match_list_coil = new List<int>();
        //public List<int> match_list_furnace = new List<int>();
        public void gen()
        {
            weight = random.Next(1, Max_Weight);
            //Console.WriteLine("weight=" + weight);
            height = random.Next(1, Max_Height);
            //Console.WriteLine("height=" + height);
            PRI = random.Next(1, Max_PRI);
            match_number_c = random.Next(0, TS_Solver.CoilNumber - 1);
            match_number_f = random.Next(TS_Solver.FurnaceNumber);
        }

        public int getWeight()
        {
            return weight;
        }

        public int getHeight()
        {
            return height;
        }

        public int getPRI()
        {
            return PRI;
        }

    }

    public class Furnace
    {
        static int GetRandomSeed()
        {
            byte[] bytes = new byte[100];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        Random random = new Random(GetRandomSeed());
        static int Max_Height = 20;
        static int Max_Number = 4;
        private static int Height;
        private static int Number;
        public void gen()
        {
            Height = random.Next(Max_Height / 2, Max_Height);
            Number = 4;
            //Number = random.Next(1, Max_Number);
            //Console.WriteLine("Height=" + Height);
        }
        public int getHeight()
        {
            return Height;
        }
        public int getNumber()
        {
            return Number;
        }
    }

    public class Relation
    {
        static int GetRandomSeed()
        {
            byte[] bytes = new byte[100];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        Random random = new Random(GetRandomSeed());
        //i and j 是在一起的收益
        //coil i 在furnace f中的成本
        static int Max_Profit = 20;
        static int Max_Cost = 15;

        int[][] Profit_if = new int[TS_Solver.CoilNumber][];
        int[][] Cost_if = new int[TS_Solver.CoilNumber][];

        public Relation()
        {
            #region the profit
            for (int i = 0; i < Profit_if.Length; i++)
            {
                for (int j = 0; j < Profit_if[i].Length; j++)
                {
                    int temp = random.Next(1, Max_Profit);
                    Profit_if[i][j] = Profit_if[j][i] = temp;
                }
            }
            #endregion

            #region the cost
            for (int i = 0; i < Cost_if.Length; i++)
            {
                for (int f = 0; f < Cost_if[i].Length; f++)
                {
                    Cost_if[i][f] = random.Next(1, Max_Cost);
                }
            }
            #endregion


        }







    }
    public class TS_Solver
    {
        static int GetRandomSeed()
        {
            byte[] bytes = new byte[100];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        Random random = new Random(GetRandomSeed());
        public static int CoilNumber = 10;
        public static int FurnaceNumber = 2;
        public Coil[] coil = new Coil[CoilNumber];
        public Furnace[] furnaces = new Furnace[FurnaceNumber];
        public int Tabu_Tenure = 5;
        public List<int> coil_index = new List<int>();


        List<int>[] solution = new List<int>[FurnaceNumber];

        public bool isContain2(int a, List<int>[] solution)
        {
            for (int i = 0; i < solution.Length; i++)
            {
                for (int j = 0; j < solution[i].Count; j++)
                {
                    if (a == solution[i][j])
                    {
                        return true;
                    }

                }
                break;
            }
            return false;

        }

        public void solver()
        {
            for (int i = 0; i < CoilNumber; i++)
            {
                coil[i] = new Coil();
                coil[i].gen();
            }

            for (int i = 0; i < FurnaceNumber; i++)
            {
                furnaces[i] = new Furnace();
                furnaces[i].gen();
            }

            for (int i = 0; i < CoilNumber; i++)
            {
                coil_index.Add(i);
            }

            #region 初始解
            //对每个furnace在不超过高度限制的前提下随机生成Coil序列
            for (int i = 0; i < FurnaceNumber; i++)//对每一个furnace找一个初始序列
            {
                Console.WriteLine("furnace{0}", i);
                Console.WriteLine("furnace_height=" + furnaces[i].getHeight());
                solution[i] = new List<int>();
                int sum_height = 0;
                int count = 0;
                /*while (count < furnaces[i].getNumber())
                {*/
                //每个furnace其实不需要装满，只要coil不重复，高度满足就好
                //只是一个初始解而已
                int temp_index = random.Next(CoilNumber);//随机选一个coil                   
                while (!solution[i].Contains(temp_index) &&
                (sum_height + coil[temp_index].getHeight() < furnaces[i].getHeight())
                && !isContain2(temp_index, solution) && count < furnaces[i].getNumber())
                {
                    solution[i].Add(temp_index);
                    Console.WriteLine("coil_index=" + temp_index);
                    sum_height += coil[temp_index].getHeight();
                    coil_index.Remove(temp_index);
                    temp_index = random.Next(CoilNumber);
                    count++;
                }
                /* }*/
                Console.WriteLine("-----sum_height=" + sum_height);

            }

            #endregion

            #region
            #endregion

        }



    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chess : MonoBehaviour
{
    // 定义三个锚点，用来判断棋子落点
    public GameObject LeftTop;
    public GameObject RightTop;
    public GameObject LeftBottom;

    //主摄像机
    public new Camera camera;
    // 锚点在屏幕上的映射位置
    Vector3 LTPos;
    Vector3 RTPos;
    Vector3 LBPos;

	Vector3 PointPos;// 当前点选的位置

	float gridWidth = 1; // 棋盘网格宽度
	float gridHeight = 1; // 棋盘网格高度
	float minGridDis;  // 网格宽和高中较小的一个

	Vector2[,] Chess_empty; // 存储棋盘上所有可以落子的位置
	int[,] Chess_state; // 存储棋盘位置上的落子状态
	enum Turn { black, white };
	Turn Chess_turn; // 落子顺序

	public GameObject White; // 白棋棋子
	public GameObject Black; // 黑棋棋子
	public Texture2D blackWin; // 白子获胜提示图
	public Texture2D whiteWin; // 黑子获胜提示图
	public int winner = 0; // 获胜方，1为黑子，-1为白子
	bool isPlaying = true; // 是否处于对弈状态
    
    //新增
	private Ready re = new Ready(15, 15);
	public static bool[,,] wins;
	public static int[] myWin;//我方第i中赢法中，有n颗棋子，如果有5颗棋子代表已经赢了，例如：myWin[5]=2;代表第5中赢法中，已经有2颗棋子,
	public static int[] computerWin;//计算机方第i中赢法中，有n颗棋子
    public static int[,] chessBorad;//代表棋盘，值0代表没有下棋子，其他值代表已经下棋子了。
    public static bool myFirst = true;
    MyList<State> states = new MyList<State>();
    //end
    public int[,] Chess_num;   //记录棋盘上落点下的是第几颗棋子
    public int Chess_order;   //记录场上现有棋子

    public Texture2D red; // 红色十字

    int Random_End_time;  //随机的一个下棋时间
    Turn Player_turn; // 玩家选择的先后手
    bool First_reward = false;    //当电脑先手时，让它固定下在天元处的开关。

    public int select = 0;
    public int regret = 0;

    public string[] datas;

    void Start()
	{
		Chess_empty = new Vector2[15, 15];    //定义棋盘
		Chess_state = new int[15, 15];    //棋盘落点状态    //新增这里改成16，16了
        Chess_num = new int[15, 15];
        Chess_turn = Turn.black;   //谁的回合
		// 计算锚点位置
		LTPos = camera.WorldToScreenPoint(LeftTop.transform.position);
		RTPos = camera.WorldToScreenPoint(RightTop.transform.position);
		LBPos = camera.WorldToScreenPoint(LeftBottom.transform.position);
		// 计算网格宽度
		gridWidth = (RTPos.x - LTPos.x) / 14;
		gridHeight = (LTPos.y - LBPos.y) / 14;
		minGridDis = gridWidth < gridHeight ? gridWidth : gridHeight;
		// 计算落子点位置
		for (int i = 0; i < 15; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				Chess_empty[i, j] = new Vector2(LBPos.x + gridWidth * i, LBPos.y + gridHeight * j);
			}
		}

        //新增,将原来的初始化的四条语句放进了函数
        Init();
        //end
    }

    void Update()
    {
        int judge;   //新增，我将judge定义在这了
        Choice_first(select); // 选择先后手
        if (select == 1 || select == -1)
        {
            if (isPlaying && Input.GetMouseButtonDown(0) && Chess_turn == Player_turn)
            {
                Player(); // 玩家的下棋判断
            }

            //新增
            else if (isPlaying && Chess_turn != Player_turn)   //暂时先用白棋表示是电脑在下棋
            {
                int[] X_And_Y = new int[2];
                Delay_Time();
                X_And_Y = Computer();  //电脑的回合，此函数可以计算电脑的落子位置
                WinJudge(X_And_Y[0], X_And_Y[1]);
            }
            //电脑下棋end
        }
        // 调用判断函数，确定是否有获胜方
        judge = Result();
        Judge(judge);
    }

    //新增初始化函数
    void Init()
    {
        wins = re.initChess();
        myWin = re.getMyWin();
        computerWin = re.getConputerWin();
        chessBorad = re.getChessBorad();
    }
    //end

    void Delay_Time()
    {
        if (Chess_order >= 51) Random_End_time = UnityEngine.Random.Range(1200, 2500);
        else if (Chess_order >= 25 && Chess_order <= 50) Random_End_time = UnityEngine.Random.Range(600, 1500); //用来在1到4里面生成一个随机的浮点数，但是不包括4
        else Random_End_time = UnityEngine.Random.Range(300, 800); //用来在1到4里面生成一个随机的浮点数，但是不包括4
        int End_time = Random_End_time;
        Debug.Log("End_time=" + End_time);
        System.Threading.Thread.Sleep(End_time);
    }

    // 计算平面距离函数
    float Dis(Vector3 mPos, Vector2 gridPos)
    {   // Mathf.Sqrt（）的平方根  Mathf.Pow（）计算次方
        return Mathf.Sqrt(Mathf.Pow(mPos.x - gridPos.x, 2) + Mathf.Pow(mPos.y - gridPos.y, 2));
    }

    // 绘制模块
    void OnGUI()
    {
        // 红十字表示最新下棋位置
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (Chess_num[i, j] == Chess_order && Chess_order != 0)
                {
                    GUI.DrawTexture(new Rect(Chess_empty[i, j].x - gridWidth / 4, Screen.height - Chess_empty[i, j].y - gridHeight / 4, gridWidth/2, gridHeight/2), red);
                }
            }
        }
        //根据获胜状态，弹出相应的胜利图片
        if (winner == 1)
            GUI.DrawTexture(new Rect(Screen.width * 0.765f, Screen.height * 0.45f, Screen.width * 0.2f, Screen.height * 0.1f), blackWin);
        if (winner == -1)
            GUI.DrawTexture(new Rect(Screen.width * 0.765f, Screen.height * 0.45f, Screen.width * 0.2f, Screen.height * 0.1f), whiteWin);
        //GUI.DrawTexture是unity绘制函数的api
    }

    void Chess_down(int x, int y)
    {
        if (Chess_state[x, y] == 2) // 当对应位置为2时，在该位置生成一个黑色棋子
        {
            GameObject B =  GameObject.Instantiate(Black, new Vector3(gridWidth * (x - 7) / 9, -20, gridHeight * (y - 7) / 9), new Quaternion(0, 0, 0, 0));
            B.name = Black.name;
            B.name = B.name.Replace("Black", "");
            B.name = B.name.Insert(0,Chess_order.ToString());
            Chess_state[x, y] = 1; // 用以储存该位置为黑棋，且避免重复生成新的棋子
            Chess_num[x, y] = ++Chess_order; // 记录当前落点上下的是第几颗棋子
        }
        else if (Chess_state[x, y] == -2) // 当对应位置为-2时，在该位置生成一个白色棋子
        {
            GameObject W = GameObject.Instantiate(White, new Vector3(gridWidth * (x - 7) / 9, -20, gridHeight * (y - 7) / 9), new Quaternion(0, 0, 0, 0));
            W.name = White.name;
            W.name = W.name.Replace("White", "");
            W.name = W.name.Insert(0, Chess_order.ToString());
            Chess_state[x, y] = -1; // 用以储存该位置为黑棋，且避免重复生成新的棋子
            Chess_num[x, y] = ++Chess_order; // 记录当前落点上下的是第几颗棋子
        }
    }

    //新增
    void WinJudge(int x, int y)
    {
        //Debug.Log("传参x="+x+"   y="+y);
        //int flag = 0;
        //下棋之前记住改变的状态
        State s;
        //s = new State();
        if (states.count >= 6)
        {
            s = states.getAndRemoveLast();
            s.k_valueList = new ArrayList();
        }
        else
        {
            s = new State();
        }
        s.x = x;
        s.y = y;
        for (int k = 0; k < re.getCount(); k++)
        {//遍历所有的赢法
            if (wins[x, y, k])
            {//如果（x,y）这个点在某一种赢法中
                //Debug.Log("找到了"+x+"  "+y+"在"+k+"个赢法中");
                //记录之前的k中赢发的状态值
                s.k_valueList.Add(new K_Value(k, myWin[k], computerWin[k]));

                computerWin[k]++;  //那么该种赢法中有多了一个棋子
                //Debug.Log("computerWin["+k+"]="+computerWin[k]);
                myWin[k] = 999;  //那么我方的这种赢法就不可能赢了，设一个异常的值
                //if (computerWin[k] == 5)
                //{ //如果计算机在某种赢法上连上了五个子，那么计算机就赢了，我方就输了
                //    flag = -1;
                //    return flag;
                //}
            }
        }
        states.add(s);
        //return flag;
    }
    //end

        // 检测是够获胜的函数result，不含黑棋禁手检测
    int Result()
    {
        int flag = 0;
        // 如果当前该白棋落子，标定黑棋刚刚下完一步，此时应该判断黑棋是否获胜
        if (Chess_turn == Turn.white)
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (i < 11)
                    {
                        flag = Black_Td(i, j);
                        if (flag == 1)
                        {
                            return flag;
                        }
                    }
                    if (j < 11)
                    {
                        flag = Black_Ld(i, j);
                        if (flag == 1)
                        {
                            return flag;
                        }
                    }
                    if (i < 11 && j < 11)
                    {
                        flag = Black_Lsd(i, j);
                        if (flag == 1)
                        {
                            return flag;
                        }
                    }
                    if (i > 3 && j < 11)
                    {
                        flag = Black_Tsd(i, j);
                        if (flag == 1)
                        {
                            return flag;
                        }
                    }
                }
            }
        }
        // 如果当前该黑棋落子，标定白棋刚刚下完一步，此时应该判断白棋是否获胜
        else if (Chess_turn == Turn.black)
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (i < 11)
                    {
                        flag = White_Td(i, j);
                        if (flag == -1)
                        {
                            return flag;
                        }
                    }
                    if (j < 11)
                    {
                        flag = White_Ld(i, j);
                        if (flag == -1)
                        {
                            return flag;
                        }
                    }
                    if (i < 11 && j < 11)
                    {
                        flag = White_Lsd(i, j);
                        if (flag == -1)
                        {
                            return flag;
                        }
                    }
                    if (i > 3 && j < 11)
                    {
                        flag = White_Tsd(i, j);
                        if (flag == -1)
                        {
                            return flag;
                        }
                    }
                }
            }
        }
        return flag;
    }

    // 白棋纵向判断
    int White_Ld(int i, int j)
    {
        return Chess_state[i, j] == -1 && Chess_state[i, j + 1] == -1 && Chess_state[i, j + 2] == -1 && Chess_state[i, j + 3] == -1 && Chess_state[i, j + 4] == -1 ? -1 : 0;
    }
    // 白棋横向判断
    int White_Td(int i, int j)
    {
        return Chess_state[i, j] == -1 && Chess_state[i + 1, j] == -1 && Chess_state[i + 2, j] == -1 && Chess_state[i + 3, j] == -1 && Chess_state[i + 4, j] == -1 ? -1 : 0;
    }
    // 白棋正斜判断
    int White_Lsd(int i, int j)
    {
        return Chess_state[i, j] == -1 && Chess_state[i + 1, j + 1] == -1 && Chess_state[i + 2, j + 2] == -1 && Chess_state[i + 3, j + 3] == -1 && Chess_state[i + 4, j + 4] == -1 ? -1 : 0;
    }
    // 白棋反斜判断
    int White_Tsd(int i, int j)
    {
        return Chess_state[i, j] == -1 && Chess_state[i - 1, j + 1] == -1 && Chess_state[i - 2, j + 2] == -1 && Chess_state[i - 3, j + 3] == -1 && Chess_state[i - 4, j + 4] == -1 ? -1 : 0;
    }
    // 黑棋横向判断
    int Black_Td(int i, int j)
    {
        return Chess_state[i, j] == 1 && Chess_state[i + 1, j] == 1 && Chess_state[i + 2, j] == 1 && Chess_state[i + 3, j] == 1 && Chess_state[i + 4, j] == 1 ? 1 : 0;
    }
    // 黑棋纵向判断
    int Black_Ld(int i, int j)
    {
        return Chess_state[i, j] == 1 && Chess_state[i, j + 1] == 1 && Chess_state[i, j + 2] == 1 && Chess_state[i, j + 3] == 1 && Chess_state[i, j + 4] == 1 ? 1 : 0;
    }
    // 黑棋正斜判断
    int Black_Lsd(int i, int j)
    {
        return Chess_state[i, j] == 1 && Chess_state[i + 1, j + 1] == 1 && Chess_state[i + 2, j + 2] == 1 && Chess_state[i + 3, j + 3] == 1 && Chess_state[i + 4, j + 4] == 1 ? 1 : 0;
    }
    // 黑棋反斜判断
    int Black_Tsd(int i, int j)
    {
        return Chess_state[i, j] == 1 && Chess_state[i - 1, j + 1] == 1 && Chess_state[i - 2, j + 2] == 1 && Chess_state[i - 3, j + 3] == 1 && Chess_state[i - 4, j + 4] == 1 ? 1 : 0;
    }

    void Player()
    {
        if (isPlaying && Input.GetMouseButtonDown(0) && Chess_turn == Player_turn)
        {
            //Debug.Log("黑棋的回合\n");
            PointPos = Input.mousePosition;    //获取鼠标坐标

            //新增
            State s;
            if (states.count >= 6)
            {
                s = states.getAndRemoveLast();
                s.k_valueList = new ArrayList();
            }
            else
            {
                s = new State();
            }
            //end

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    // 找到最接近鼠标点击位置的落子点，如果空则落子
                    if (Dis(PointPos, Chess_empty[i, j]) < minGridDis / 2 && Chess_state[i, j] == 0)
                    {
                        // 根据下棋顺序确定落子颜色
                        Chess_state[i, j] = Chess_turn == Turn.black ? 2 : -2;
                        // 落子成功，更换下棋顺序
                        Chess_turn = Chess_turn == Turn.black ? Turn.white : Turn.black;   //落子成功改回合状态

                        //新增的
                        s.x = i;
                        s.y = j;
                        //end
                    }
                    Chess_down(s.x, s.y);
                }
            }
            //新增   分析这里的意思是如果玩家将棋子下在此处，则人机将不会在这种赢法中获胜，因此mywin即玩家在该种赢法中的棋子数加一
            //，而我方不可能取得该种赢法的胜利，则标记一个异常值

            //end
            for (int k = 0; k < re.getCount(); k++)
            {
                if (wins[s.x, s.y, k])
                {
                    //直接之前的k中赢发的状态值

                    s.k_valueList.Add(new K_Value(k, myWin[k], computerWin[k]));
                    myWin[k]++;
                    Debug.Log("1.myWin[" + k + "]=" + myWin[k]);
                    computerWin[k] = 999;
                }
            }
            states.add(s);
        }
    }
    int[] Computer()
    {
        Debug.Log("白棋的回合");
        int[] X_And_Y = new int[2];
        int[,] myScore = new int[15, 15];     //评估我方评分
        int[,] computerScore = new int[15, 15];     //评估计算机评分
        long max = 0;
        int x = 0;
        int y = 0;

        if (Player_turn != Turn.black && First_reward == false)
        {
            Debug.Log("电脑先手");
            Chess_state[7, 7] = Chess_turn == Turn.black ? 2 : -2;
            // 落子成功，更换下棋顺序
            Chess_turn = Chess_turn == Turn.black ? Turn.white : Turn.black;   //落子成功改回合状态
            Chess_down(7, 7);
            X_And_Y[0] = 7;
            X_And_Y[1] = 7;
            First_reward = true;
            return X_And_Y;
        }

        // 遍历棋盘
        for (int i = 0; i < 15; i++)
        {//x轴
            for (int j = 0; j < 15; j++)
            {//y轴
             //Debug.Log("遍历棋盘\n");
                if (Chess_state[i, j] != 1 && Chess_state[i, j] != -1)
                {// 当前可落子，如果不等于0代表已经有棋子
                    for (int k = 0; k < re.getCount(); k++)
                    { // 每个点都在 多种赢法中，所以要遍历所有赢法
                      //Debug.Log("遍历赢法\n");
                        if (wins[i, j, k])
                        {// 计算他在K赢法中的 重要性
                         //Debug.Log("wins["+i+","+j+","+k+"]="+wins[i, j, k]);
                            if (myWin[k] >= 1) Debug.Log("2.myWin[" + k + "]=" + myWin[k]);
                            switch (myWin[k])
                            {//我方棋路，当前赢法中已经连上了几颗棋子
                                case 1:
                                    myScore[i, j] += 200;
                                    break;
                                case 2:
                                    myScore[i, j] += 400;
                                    break;
                                case 3:
                                    myScore[i, j] += 2000;
                                    break;
                                case 4:
                                    myScore[i, j] += 10000;
                                    break;
                            }
                            if (myScore[i, j] >= 1) Debug.Log("1.myScore=" + myScore[i, j]);
                            switch (computerWin[k])
                            {//计算机棋路，当前赢法中已经连上了几颗棋子
                                case 1:
                                    computerScore[i, j] += 300;
                                    break;
                                case 2:
                                    computerScore[i, j] += 500;
                                    break;
                                case 3:
                                    computerScore[i, j] += 5000;
                                    break;
                                case 4:
                                    computerScore[i, j] += 20000;
                                    break;
                            }
                            if (computerScore[i, j] >= 1) Debug.Log("1.computerScore=" + computerScore[i, j]);
                        }
                    }
                    if (myScore[i, j] >= 1) Debug.Log("2.myScore=" + myScore[i, j]);
                    // 玩家最重要的落点
                    if (myScore[i, j] > max)
                    {
                        max = myScore[i, j];
                        x = i;
                        y = j;
                        Debug.Log("1.max=" + max + "   x=" + x + "  y=" + y);
                    }
                    else if (myScore[i, j] == max)
                    {
                        if (computerScore[i, j] > computerScore[x, y])
                        { // 
                            x = i;
                            y = j;
                            Debug.Log("2.max=" + max + "   x=" + x + "  y=" + y);
                        }
                    }
                    if (computerScore[i, j] >= 1) Debug.Log("2.computerScore=" + computerScore[i, j]);
                    // AI最重要的落点
                    if (computerScore[i, j] > max)
                    {
                        max = computerScore[i, j];
                        x = i;
                        y = j;
                        Debug.Log("3.max=" + max + "   x=" + x + "  y=" + y);
                    }
                    else if (computerScore[i, j] == max)
                    {
                        if (myScore[i, j] > myScore[x, y])
                        {
                            x = i;
                            y = j;
                            Debug.Log("4.max=" + max + "   x=" + x + "  y=" + y);
                        }
                    }
                }
            }
        }
        Debug.Log("电脑下棋前的条件判断结果：" + Chess_state[x, y] + "  &&  " + Chess_turn + "  x=" + x + "  y=" + y);
        if (Chess_state[x, y] == 0 && Chess_turn != Player_turn)
        {
            // 根据下棋顺序确定落子颜色
            //Debug.Log("电脑找到了落点\n");
            Debug.Log("电脑落子位置x=" + x + "   y=" + y);
            Chess_state[x, y] = Chess_turn == Turn.black ? 2 : -2;
            // 落子成功，更换下棋顺序
            Chess_turn = Chess_turn == Turn.black ? Turn.white : Turn.black;   //落子成功改回合状态
            
        }
        Chess_down(x, y);
        X_And_Y[0] = x;
        X_And_Y[1] = y;
        return X_And_Y;
    }

    public void Judge(int judge)  // 判断胜负模块
    {
        if (judge == 2)
        {
            int New_GetKey = Player_turn == Turn.white ? 1 : -1;
            if (New_GetKey == 1)
            {
                //Debug.Log("黑棋胜");
                winner = 1;
                isPlaying = false;
            }
            else if (New_GetKey == -1)
            {
                //Debug.Log("白棋胜");
                winner = -1;
                isPlaying = false;
            }
        }
        else if (judge == 1)
        {
            //Debug.Log("黑棋胜");
            winner = 1;
            isPlaying = false;
        }
        else if (judge == -1)
        {
            //Debug.Log("白棋胜");
            winner = -1;
            isPlaying = false;
        }
    }
    public int Choice_first(int GetKey)  // 选先后手模块
    {
            if (GetKey == 1)  //意为选择先手
            {
                Player_turn = Turn.black;
                select = 1;
            }
            else if (GetKey == -1)
            {
                Player_turn = Turn.white;
                select = -1;
            }
        return 0;
    }
    public void Regret_1() // 悔棋模块
    {
        if (isPlaying)
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (Chess_num[i, j] == Chess_order && Chess_order != 0)
                    {
                        Chess_state[i, j] = 0;
                        Chess_num[i, j] = 0;
                    }
                    if (Chess_num[i, j] == Chess_order - 1 && Chess_order != 1)
                    {
                        Chess_state[i, j] = 0;
                        Chess_num[i, j] = 0;
                    }
                }
            }
            GameObject.Find((Chess_order - 1).ToString()).GetComponent<DestroyObj>().Destroy();
            GameObject.Find((Chess_order - 2).ToString()).GetComponent<DestroyObj>().Destroy();
            Chess_order -= 2;
        }
    }
}






//新增   数据结构，容器，工具等
class Ready
{
    public int x;
    public int y;
    public int count;
    public int getCount()
    {
        return this.count;
    }
    /**
     * 计算所有赢法种类
     * @return
     */
    public Ready(int x, int y)
    {
        this.x = x;
        this.y = y;
        count = initCount();
    }
    private int initCount()
    {
        int count = 0;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y - 4; j++)
            {
                count++;
            }
        }
        // 横线上
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y - 4; j++)
            {
                count++;
            }
        }
        // 斜线上
        for (int i = 0; i < x - 4; i++)
        {
            for (int j = 0; j < y - 4; j++)
            {
                count++;
            }
        }
        // 反斜线上
        for (int i = 0; i < x - 4; i++)
        {
            for (int j = y - 1; j > 3; j--)
            {
                count++;
            }
        }
        return count;
    }

    /**
     * 初始化所有赢法
     * @param c
     * @return
     */
    public bool[,,] initChess()
    {
        bool[,,] wins = new bool[x, y, count];
        int counts = 0;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y - 4; j++)
            {
                for (int k = 0; k < 5; k++)
                {
                    wins[i, j + k, counts] = true;
                }
                counts++;
            }
        }
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y - 4; j++)
            {
                for (int k = 0; k < 5; k++)
                {
                    wins[j + k, i, counts] = true;
                }
                counts++;
            }
        }
        for (int i = 0; i < x - 4; i++)
        {
            for (int j = 0; j < y - 4; j++)
            {
                for (int k = 0; k < 5; k++)
                {
                    wins[i + k, j + k, counts] = true;
                }
                counts++;
            }
        }
        for (int i = 0; i < x - 4; i++)
        {
            for (int j = y - 1; j > 3; j--)
            {
                for (int k = 0; k < 5; k++)
                {
                    wins[i + k, j - k, counts] = true;//记录赢得可能性
                }
                counts++;
            }
        }
        return wins;
    }


    public int[] getMyWin()
    {
        int[] myWin = new int[count];
        return myWin;
    }

    public int[] getConputerWin()
    {
        int[] conputer = new int[count];
        return conputer;
    }
    /**
     * 初始化棋盘
     * @return
     */
    public int[,] getChessBorad()
    {
        int[,] chessBorad = new int[x, y];
        return chessBorad;
    }
}


class State
{
    public int x;
    public int y;
    //public PictureBox pictureBox;
    public ArrayList k_valueList = new ArrayList();
}

class K_Value
{
    public K_Value(int k, int myWinValue, int computerWinValue)
    {
        this.k = k;
        this.myWinValue = myWinValue;
        this.computerWinValue = computerWinValue;
    }
    public int k;
    public int myWinValue;
    public int computerWinValue;
}

//自制链表
class MyList<T>
{
    public int count;//总数
    public Node<T> first;//头节点
    public Node<T> last;//尾节点
                        //添加函数
    public void add(T t)
    {
        if (first == null)
        {
            first = new Node<T>(t);
            last = first;
        }
        else
        {
            Node<T> node = new Node<T>(t);
            node.next = first;
            first.prev = node;
            first = node;
        }
        count++;
    }
    /**
     * 获取并删除最后一个元素
     * **/
    public T getAndRemoveLast()
    {
        Node<T> las = last;
        if (last != null)
        {
            count--;
            if (first == last)
            {
                last = null;
                first = last;
                return las.t;
            }
            else
            {
                last = last.prev;
                las.prev = null;
                last.next = null;
                return las.t;
            }
        }
        else
        {
            return default(T);
        }
    }


    /**
     * 获取并删除第一个元素
     * **/
    public T getAndRemoveFirst()
    {
        Node<T> fir = first;
        if (first != null)
        {
            count--;
            if (first == last)
            {
                last = null;
                first = last;
                return fir.t;
            }
            else
            {
                first = first.next;
                fir.next = null;
                first.prev = null;
                return fir.t;
            }
        }
        else
        {
            return default(T);
        }
    }
}

class Node<T>
{
    public Node<T> next;
    public Node<T> prev;
    public T t;
    public Node(T t)
    {
        this.t = t;
        next = null;
        prev = null;
    }
}
//end
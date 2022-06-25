using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chess : MonoBehaviour
{
    // ��������ê�㣬�����ж��������
    public GameObject LeftTop;
    public GameObject RightTop;
    public GameObject LeftBottom;

    //�������
    public new Camera camera;
    // ê������Ļ�ϵ�ӳ��λ��
    Vector3 LTPos;
    Vector3 RTPos;
    Vector3 LBPos;

	Vector3 PointPos;// ��ǰ��ѡ��λ��

	float gridWidth = 1; // ����������
	float gridHeight = 1; // ��������߶�
	float minGridDis;  // �����͸��н�С��һ��

	Vector2[,] Chess_empty; // �洢���������п������ӵ�λ��
	int[,] Chess_state; // �洢����λ���ϵ�����״̬
	enum Turn { black, white };
	Turn Chess_turn; // ����˳��

	public GameObject White; // ��������
	public GameObject Black; // ��������
	public Texture2D blackWin; // ���ӻ�ʤ��ʾͼ
	public Texture2D whiteWin; // ���ӻ�ʤ��ʾͼ
	public int winner = 0; // ��ʤ����1Ϊ���ӣ�-1Ϊ����
	bool isPlaying = true; // �Ƿ��ڶ���״̬
    
    //����
	private Ready re = new Ready(15, 15);
	public static bool[,,] wins;
	public static int[] myWin;//�ҷ���i��Ӯ���У���n�����ӣ������5�����Ӵ����Ѿ�Ӯ�ˣ����磺myWin[5]=2;�����5��Ӯ���У��Ѿ���2������,
	public static int[] computerWin;//���������i��Ӯ���У���n������
    public static int[,] chessBorad;//�������̣�ֵ0����û�������ӣ�����ֵ�����Ѿ��������ˡ�
    public static bool myFirst = true;
    MyList<State> states = new MyList<State>();
    //end
    public int[,] Chess_num;   //��¼����������µ��ǵڼ�������
    public int Chess_order;   //��¼������������

    public Texture2D red; // ��ɫʮ��

    int Random_End_time;  //�����һ������ʱ��
    Turn Player_turn; // ���ѡ����Ⱥ���
    bool First_reward = false;    //����������ʱ�������̶�������Ԫ���Ŀ��ء�

    public int select = 0;
    public int regret = 0;

    public string[] datas;

    void Start()
	{
		Chess_empty = new Vector2[15, 15];    //��������
		Chess_state = new int[15, 15];    //�������״̬    //��������ĳ�16��16��
        Chess_num = new int[15, 15];
        Chess_turn = Turn.black;   //˭�Ļغ�
		// ����ê��λ��
		LTPos = camera.WorldToScreenPoint(LeftTop.transform.position);
		RTPos = camera.WorldToScreenPoint(RightTop.transform.position);
		LBPos = camera.WorldToScreenPoint(LeftBottom.transform.position);
		// ����������
		gridWidth = (RTPos.x - LTPos.x) / 14;
		gridHeight = (LTPos.y - LBPos.y) / 14;
		minGridDis = gridWidth < gridHeight ? gridWidth : gridHeight;
		// �������ӵ�λ��
		for (int i = 0; i < 15; i++)
		{
			for (int j = 0; j < 15; j++)
			{
				Chess_empty[i, j] = new Vector2(LBPos.x + gridWidth * i, LBPos.y + gridHeight * j);
			}
		}

        //����,��ԭ���ĳ�ʼ�����������Ž��˺���
        Init();
        //end
    }

    void Update()
    {
        int judge;   //�������ҽ�judge����������
        Choice_first(select); // ѡ���Ⱥ���
        if (select == 1 || select == -1)
        {
            if (isPlaying && Input.GetMouseButtonDown(0) && Chess_turn == Player_turn)
            {
                Player(); // ��ҵ������ж�
            }

            //����
            else if (isPlaying && Chess_turn != Player_turn)   //��ʱ���ð����ʾ�ǵ���������
            {
                int[] X_And_Y = new int[2];
                Delay_Time();
                X_And_Y = Computer();  //���ԵĻغϣ��˺������Լ�����Ե�����λ��
                WinJudge(X_And_Y[0], X_And_Y[1]);
            }
            //��������end
        }
        // �����жϺ�����ȷ���Ƿ��л�ʤ��
        judge = Result();
        Judge(judge);
    }

    //������ʼ������
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
        else if (Chess_order >= 25 && Chess_order <= 50) Random_End_time = UnityEngine.Random.Range(600, 1500); //������1��4��������һ������ĸ����������ǲ�����4
        else Random_End_time = UnityEngine.Random.Range(300, 800); //������1��4��������һ������ĸ����������ǲ�����4
        int End_time = Random_End_time;
        Debug.Log("End_time=" + End_time);
        System.Threading.Thread.Sleep(End_time);
    }

    // ����ƽ����뺯��
    float Dis(Vector3 mPos, Vector2 gridPos)
    {   // Mathf.Sqrt������ƽ����  Mathf.Pow��������η�
        return Mathf.Sqrt(Mathf.Pow(mPos.x - gridPos.x, 2) + Mathf.Pow(mPos.y - gridPos.y, 2));
    }

    // ����ģ��
    void OnGUI()
    {
        // ��ʮ�ֱ�ʾ��������λ��
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
        //���ݻ�ʤ״̬��������Ӧ��ʤ��ͼƬ
        if (winner == 1)
            GUI.DrawTexture(new Rect(Screen.width * 0.765f, Screen.height * 0.45f, Screen.width * 0.2f, Screen.height * 0.1f), blackWin);
        if (winner == -1)
            GUI.DrawTexture(new Rect(Screen.width * 0.765f, Screen.height * 0.45f, Screen.width * 0.2f, Screen.height * 0.1f), whiteWin);
        //GUI.DrawTexture��unity���ƺ�����api
    }

    void Chess_down(int x, int y)
    {
        if (Chess_state[x, y] == 2) // ����Ӧλ��Ϊ2ʱ���ڸ�λ������һ����ɫ����
        {
            GameObject B =  GameObject.Instantiate(Black, new Vector3(gridWidth * (x - 7) / 9, -20, gridHeight * (y - 7) / 9), new Quaternion(0, 0, 0, 0));
            B.name = Black.name;
            B.name = B.name.Replace("Black", "");
            B.name = B.name.Insert(0,Chess_order.ToString());
            Chess_state[x, y] = 1; // ���Դ����λ��Ϊ���壬�ұ����ظ������µ�����
            Chess_num[x, y] = ++Chess_order; // ��¼��ǰ������µ��ǵڼ�������
        }
        else if (Chess_state[x, y] == -2) // ����Ӧλ��Ϊ-2ʱ���ڸ�λ������һ����ɫ����
        {
            GameObject W = GameObject.Instantiate(White, new Vector3(gridWidth * (x - 7) / 9, -20, gridHeight * (y - 7) / 9), new Quaternion(0, 0, 0, 0));
            W.name = White.name;
            W.name = W.name.Replace("White", "");
            W.name = W.name.Insert(0, Chess_order.ToString());
            Chess_state[x, y] = -1; // ���Դ����λ��Ϊ���壬�ұ����ظ������µ�����
            Chess_num[x, y] = ++Chess_order; // ��¼��ǰ������µ��ǵڼ�������
        }
    }

    //����
    void WinJudge(int x, int y)
    {
        //Debug.Log("����x="+x+"   y="+y);
        //int flag = 0;
        //����֮ǰ��ס�ı��״̬
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
        {//�������е�Ӯ��
            if (wins[x, y, k])
            {//�����x,y���������ĳһ��Ӯ����
                //Debug.Log("�ҵ���"+x+"  "+y+"��"+k+"��Ӯ����");
                //��¼֮ǰ��k��Ӯ����״ֵ̬
                s.k_valueList.Add(new K_Value(k, myWin[k], computerWin[k]));

                computerWin[k]++;  //��ô����Ӯ�����ж���һ������
                //Debug.Log("computerWin["+k+"]="+computerWin[k]);
                myWin[k] = 999;  //��ô�ҷ�������Ӯ���Ͳ�����Ӯ�ˣ���һ���쳣��ֵ
                //if (computerWin[k] == 5)
                //{ //����������ĳ��Ӯ��������������ӣ���ô�������Ӯ�ˣ��ҷ�������
                //    flag = -1;
                //    return flag;
                //}
            }
        }
        states.add(s);
        //return flag;
    }
    //end

        // ����ǹ���ʤ�ĺ���result������������ּ��
    int Result()
    {
        int flag = 0;
        // �����ǰ�ð������ӣ��궨����ո�����һ������ʱӦ���жϺ����Ƿ��ʤ
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
        // �����ǰ�ú������ӣ��궨����ո�����һ������ʱӦ���жϰ����Ƿ��ʤ
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

    // ���������ж�
    int White_Ld(int i, int j)
    {
        return Chess_state[i, j] == -1 && Chess_state[i, j + 1] == -1 && Chess_state[i, j + 2] == -1 && Chess_state[i, j + 3] == -1 && Chess_state[i, j + 4] == -1 ? -1 : 0;
    }
    // ��������ж�
    int White_Td(int i, int j)
    {
        return Chess_state[i, j] == -1 && Chess_state[i + 1, j] == -1 && Chess_state[i + 2, j] == -1 && Chess_state[i + 3, j] == -1 && Chess_state[i + 4, j] == -1 ? -1 : 0;
    }
    // ������б�ж�
    int White_Lsd(int i, int j)
    {
        return Chess_state[i, j] == -1 && Chess_state[i + 1, j + 1] == -1 && Chess_state[i + 2, j + 2] == -1 && Chess_state[i + 3, j + 3] == -1 && Chess_state[i + 4, j + 4] == -1 ? -1 : 0;
    }
    // ���巴б�ж�
    int White_Tsd(int i, int j)
    {
        return Chess_state[i, j] == -1 && Chess_state[i - 1, j + 1] == -1 && Chess_state[i - 2, j + 2] == -1 && Chess_state[i - 3, j + 3] == -1 && Chess_state[i - 4, j + 4] == -1 ? -1 : 0;
    }
    // ��������ж�
    int Black_Td(int i, int j)
    {
        return Chess_state[i, j] == 1 && Chess_state[i + 1, j] == 1 && Chess_state[i + 2, j] == 1 && Chess_state[i + 3, j] == 1 && Chess_state[i + 4, j] == 1 ? 1 : 0;
    }
    // ���������ж�
    int Black_Ld(int i, int j)
    {
        return Chess_state[i, j] == 1 && Chess_state[i, j + 1] == 1 && Chess_state[i, j + 2] == 1 && Chess_state[i, j + 3] == 1 && Chess_state[i, j + 4] == 1 ? 1 : 0;
    }
    // ������б�ж�
    int Black_Lsd(int i, int j)
    {
        return Chess_state[i, j] == 1 && Chess_state[i + 1, j + 1] == 1 && Chess_state[i + 2, j + 2] == 1 && Chess_state[i + 3, j + 3] == 1 && Chess_state[i + 4, j + 4] == 1 ? 1 : 0;
    }
    // ���巴б�ж�
    int Black_Tsd(int i, int j)
    {
        return Chess_state[i, j] == 1 && Chess_state[i - 1, j + 1] == 1 && Chess_state[i - 2, j + 2] == 1 && Chess_state[i - 3, j + 3] == 1 && Chess_state[i - 4, j + 4] == 1 ? 1 : 0;
    }

    void Player()
    {
        if (isPlaying && Input.GetMouseButtonDown(0) && Chess_turn == Player_turn)
        {
            //Debug.Log("����Ļغ�\n");
            PointPos = Input.mousePosition;    //��ȡ�������

            //����
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
                    // �ҵ���ӽ������λ�õ����ӵ㣬�����������
                    if (Dis(PointPos, Chess_empty[i, j]) < minGridDis / 2 && Chess_state[i, j] == 0)
                    {
                        // ��������˳��ȷ��������ɫ
                        Chess_state[i, j] = Chess_turn == Turn.black ? 2 : -2;
                        // ���ӳɹ�����������˳��
                        Chess_turn = Chess_turn == Turn.black ? Turn.white : Turn.black;   //���ӳɹ��Ļغ�״̬

                        //������
                        s.x = i;
                        s.y = j;
                        //end
                    }
                    Chess_down(s.x, s.y);
                }
            }
            //����   �����������˼�������ҽ��������ڴ˴������˻�������������Ӯ���л�ʤ�����mywin������ڸ���Ӯ���е���������һ
            //�����ҷ�������ȡ�ø���Ӯ����ʤ��������һ���쳣ֵ

            //end
            for (int k = 0; k < re.getCount(); k++)
            {
                if (wins[s.x, s.y, k])
                {
                    //ֱ��֮ǰ��k��Ӯ����״ֵ̬

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
        Debug.Log("����Ļغ�");
        int[] X_And_Y = new int[2];
        int[,] myScore = new int[15, 15];     //�����ҷ�����
        int[,] computerScore = new int[15, 15];     //�������������
        long max = 0;
        int x = 0;
        int y = 0;

        if (Player_turn != Turn.black && First_reward == false)
        {
            Debug.Log("��������");
            Chess_state[7, 7] = Chess_turn == Turn.black ? 2 : -2;
            // ���ӳɹ�����������˳��
            Chess_turn = Chess_turn == Turn.black ? Turn.white : Turn.black;   //���ӳɹ��Ļغ�״̬
            Chess_down(7, 7);
            X_And_Y[0] = 7;
            X_And_Y[1] = 7;
            First_reward = true;
            return X_And_Y;
        }

        // ��������
        for (int i = 0; i < 15; i++)
        {//x��
            for (int j = 0; j < 15; j++)
            {//y��
             //Debug.Log("��������\n");
                if (Chess_state[i, j] != 1 && Chess_state[i, j] != -1)
                {// ��ǰ�����ӣ����������0�����Ѿ�������
                    for (int k = 0; k < re.getCount(); k++)
                    { // ÿ���㶼�� ����Ӯ���У�����Ҫ��������Ӯ��
                      //Debug.Log("����Ӯ��\n");
                        if (wins[i, j, k])
                        {// ��������KӮ���е� ��Ҫ��
                         //Debug.Log("wins["+i+","+j+","+k+"]="+wins[i, j, k]);
                            if (myWin[k] >= 1) Debug.Log("2.myWin[" + k + "]=" + myWin[k]);
                            switch (myWin[k])
                            {//�ҷ���·����ǰӮ�����Ѿ������˼�������
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
                            {//�������·����ǰӮ�����Ѿ������˼�������
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
                    // �������Ҫ�����
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
                    // AI����Ҫ�����
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
        Debug.Log("��������ǰ�������жϽ����" + Chess_state[x, y] + "  &&  " + Chess_turn + "  x=" + x + "  y=" + y);
        if (Chess_state[x, y] == 0 && Chess_turn != Player_turn)
        {
            // ��������˳��ȷ��������ɫ
            //Debug.Log("�����ҵ������\n");
            Debug.Log("��������λ��x=" + x + "   y=" + y);
            Chess_state[x, y] = Chess_turn == Turn.black ? 2 : -2;
            // ���ӳɹ�����������˳��
            Chess_turn = Chess_turn == Turn.black ? Turn.white : Turn.black;   //���ӳɹ��Ļغ�״̬
            
        }
        Chess_down(x, y);
        X_And_Y[0] = x;
        X_And_Y[1] = y;
        return X_And_Y;
    }

    public void Judge(int judge)  // �ж�ʤ��ģ��
    {
        if (judge == 2)
        {
            int New_GetKey = Player_turn == Turn.white ? 1 : -1;
            if (New_GetKey == 1)
            {
                //Debug.Log("����ʤ");
                winner = 1;
                isPlaying = false;
            }
            else if (New_GetKey == -1)
            {
                //Debug.Log("����ʤ");
                winner = -1;
                isPlaying = false;
            }
        }
        else if (judge == 1)
        {
            //Debug.Log("����ʤ");
            winner = 1;
            isPlaying = false;
        }
        else if (judge == -1)
        {
            //Debug.Log("����ʤ");
            winner = -1;
            isPlaying = false;
        }
    }
    public int Choice_first(int GetKey)  // ѡ�Ⱥ���ģ��
    {
            if (GetKey == 1)  //��Ϊѡ������
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
    public void Regret_1() // ����ģ��
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






//����   ���ݽṹ�����������ߵ�
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
     * ��������Ӯ������
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
        // ������
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y - 4; j++)
            {
                count++;
            }
        }
        // б����
        for (int i = 0; i < x - 4; i++)
        {
            for (int j = 0; j < y - 4; j++)
            {
                count++;
            }
        }
        // ��б����
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
     * ��ʼ������Ӯ��
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
                    wins[i + k, j - k, counts] = true;//��¼Ӯ�ÿ�����
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
     * ��ʼ������
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

//��������
class MyList<T>
{
    public int count;//����
    public Node<T> first;//ͷ�ڵ�
    public Node<T> last;//β�ڵ�
                        //��Ӻ���
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
     * ��ȡ��ɾ�����һ��Ԫ��
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
     * ��ȡ��ɾ����һ��Ԫ��
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
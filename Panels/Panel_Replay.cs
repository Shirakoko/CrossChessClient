using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Replay : MonoBehaviour
{
    public Grid[] grids;
    public Sprite[] sprites;
    private Coroutine REPLICATE = null; // 保存协程的变量
    private GameObject Emp_OverGo;
    private int[] steps;

    void Awake()
    {
        grids = new Grid[9]; // 实例化格子对象的数组4
        Emp_OverGo = transform.Find("Emp_Over").gameObject;
    }

    void Start()
    {
        // 把格子对象加入数组，并初始化
        Transform gridParent = transform.Find("Grids");
        for(int i=0; i<9; i++)
        {
            grids[i] = gridParent.GetChild(i).GetComponent<Grid>();
            grids[i].InitGrid(i);
        }

        // 设置不可见
        Emp_OverGo.SetActive(false);

        // 添加按钮绑定事件
        Emp_OverGo.transform.Find("Btn_Back").GetComponent<Button>().onClick.AddListener(()=>{
            this.transform.localPosition += new Vector3(1920,0,0);
        });
        Emp_OverGo.transform.Find("Btn_Replay").GetComponent<Button>().onClick.AddListener(()=>{
            ReplicateRound(this.steps);
        });
    }

    public void ReplicateRound(int[] steps)
    {
        // 传递变量
        this.steps = steps;
        // 先设置蒙皮不可见
        Emp_OverGo.SetActive(false);
        // 模拟之前先清空棋盘
        foreach(Grid grid in grids)
        {
            grid.transform.GetChild(0).gameObject.SetActive(false);
        }
        // 开始模拟
        REPLICATE = StartCoroutine(ShowStone(steps));
    }

    private IEnumerator ShowStone(int[] steps)
    {
        int i;
        for(i=0; i<9;i++)
        {
            if(steps[i] == -1)
            {
                yield return new WaitForSeconds(0.5f);
                Emp_OverGo.SetActive(true);
                if(REPLICATE!=null){StopCoroutine(REPLICATE);} 
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                grids[steps[i]].ShowStone(sprites[i%2]);
            }
        }
        if(i==9)
        {
            yield return new WaitForSeconds(0.5f);
            Emp_OverGo.SetActive(true);
            if(REPLICATE!=null){StopCoroutine(REPLICATE);} 
        }
    }
}

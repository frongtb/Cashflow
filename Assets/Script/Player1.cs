using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player1 : MonoBehaviourPunCallbacks
{
    public static Player1 instace;
    public int playerid;

    [Header("ROUTES")]
    public Route commonRoute;
    public Route outerRoute;
    public Route StartRoute;

    public List<Node> outterRoute = new List<Node>();
    public List<Node> fullRoute = new List<Node>();
    [Header("NODES")]
    public Node baseNode;
    public Node startNode;
    public Node currentNode;
    public Node lastNode;

    int routePosition;
    int startNodeIndex;

    int steps;
    int doneSteps;
    public int turncounts = 1;

    [Header("BOOLS")]
    public bool isOut = true;
    bool isMoving;

    bool hasTurn;//human input


    [Header("SELECTOR")]
    public GameObject selector;

    float amplitude = 0.5f;
    float cTime = 0f;

    void Start()
    {

        startNodeIndex = commonRoute.RequestPosition(startNode.gameObject.transform);

        CreateFullRoute();

        SetSelector(false);

    }

    void CreateFullRoute()
    {
        for (int i = 0; i < commonRoute.childNodeList.Count; i++)
        {
            int tempPos = startNodeIndex + i;
            tempPos %= commonRoute.childNodeList.Count;

            fullRoute.Add(commonRoute.childNodeList[tempPos + 1].GetComponent<Node>());
        }

        for (int i = 0; i < outerRoute.childNodeList.Count; i++)
        {
            outterRoute.Add(outerRoute.childNodeList[i].GetComponent<Node>());
        }
    }

    void Update()
    {

    }

    IEnumerator Move()
    {
        Debug.Log("in move func");
        if (GameManager.instace.playerList[GameManager.instace.activePlayer].EnterOuter == true)
        {
            GameManager.instace.playerList[GameManager.instace.activePlayer].EnterOuter = false;

        }

        if (GameManager.instace.playerList[GameManager.instace.activePlayer].hasOutside == true)
        {
            fullRoute = outterRoute;
        }

        if (isMoving)
        {
            yield break;
        }
        isMoving = true;

        while (steps > 0)
        {

            if (routePosition % fullRoute.Count == 23)
            {

                GameManager.instace.playerList[GameManager.instace.activePlayer].remiderPosition = true;

            }
            routePosition++;
            routePosition %= fullRoute.Count;
            //Debug.Log(routePosition % fullRoute.Count);
            Debug.Log(routePosition);
            Debug.Log(fullRoute.Count);
            Debug.Log(routePosition % fullRoute.Count);
            Debug.Log(fullRoute[routePosition].gameObject.transform.position);
            Vector3 nextPos = fullRoute[routePosition].gameObject.transform.position;
            Vector3 startPos = fullRoute[routePosition].gameObject.transform.position;
            Debug.Log(nextPos);
            photonView.RPC("valueUpdate", RpcTarget.All);
            //while (MoveToNextNode(nextPos,8f)){yield return null;}
            while (MoveInArcToNextNode(startPos, nextPos, 8f)) { yield return null; }
            //Debug.Log(routePosition % fullRoute.Count);
            //orange pass
            if (routePosition % fullRoute.Count == 6 || routePosition % fullRoute.Count == 14 || routePosition % fullRoute.Count == 22)
            {
                Debug.Log("pass orange route");
                photonView.RPC("valueUpdate", RpcTarget.All);
                GameManager.instace.playerList[GameManager.instace.activePlayer].money = GameManager.instace.playerList[GameManager.instace.activePlayer].money + GameManager.instace.playerList[GameManager.instace.activePlayer].getmoney;
                photonView.RPC("UpdateMoney", RpcTarget.All, GameManager.instace.playerList[GameManager.instace.activePlayer].money, GameManager.instace.activePlayer);
                if (GameManager.instace.playerList[GameManager.instace.activePlayer].getmoney <= 0 && GameManager.instace.playerList[GameManager.instace.activePlayer].money < Math.Abs(GameManager.instace.playerList[GameManager.instace.activePlayer].getmoney))
                {
                    //lose
                    GameManager.instace.playerList[GameManager.instace.activePlayer].playerType = GameManager.Entity.PlayerTypes.NO_PLAYER;
                    GameManager.instace.playerInRoom = 0;
                    for (int i = 0; i < GameManager.instace.playerList.Count; i++)
                    {
                        if (GameManager.instace.playerList[i].playerType == GameManager.Entity.PlayerTypes.NO_PLAYER)
                        {
                            GameManager.instace.playerInRoom++;
                        }

                    }
                    if (GameManager.instace.playerInRoom > 0)
                    {
                        GameManager.instace.state = GameManager.States.SWITCH_PLAYER;
                    }
                    UIController.instance.BlurBg.SetActive(true);
                    UIController.instance.lostShow.SetActive(true);
                }
            }




            yield return new WaitForSeconds(0.1f);
            cTime = 0;
            steps--;
            doneSteps++;
            //Debug.Log(doneSteps);
        }


        isMoving = false;


        if (isMoving == false)
        {

            //red route
            if (routePosition % fullRoute.Count == 2 || routePosition % fullRoute.Count == 10 || routePosition % fullRoute.Count == 18)
            {
                Debug.Log("in red route");
                photonView.RPC("setUpDeckSpendToEveryone", RpcTarget.All);
                photonView.RPC("valueUpdate", RpcTarget.All);
                //UIController.instance.drawButton.SetActive(IsMyTurn());
                photonView.RPC("PlayerDraw", RpcTarget.All);
            }
            //orange route
            if (routePosition % fullRoute.Count == 6 || routePosition % fullRoute.Count == 14 || routePosition % fullRoute.Count == 22)
            {
                Debug.Log("in orange route");
                photonView.RPC("valueUpdate", RpcTarget.All);
                //UIController.instance.passButton.SetActive(IsMyTurn());
                photonView.RPC("EndTurnPlayer", RpcTarget.All);
                //GameManager.instace.state = GameManager.States.SWITCH_PLAYER;
            }

            //blue route
            if (routePosition % fullRoute.Count == 8 || routePosition % fullRoute.Count == 16
                || (routePosition % fullRoute.Count == 0 && fullRoute.Count - routePosition == fullRoute.Count && GameManager.instace.playerList[GameManager.instace.activePlayer].remiderPosition == true))
            {
                //
                Debug.Log("in blue route");
                GameManager.instace.playerList[GameManager.instace.activePlayer].remiderPosition = false;
                photonView.RPC("setUpDeckMarketToEveryone", RpcTarget.All);
                photonView.RPC("valueUpdate", RpcTarget.All);
                Debug.Log(routePosition % fullRoute.Count + " " + steps + " " + routePosition + " " + isMoving + " " + doneSteps);
                photonView.RPC("PlayerMarketDraw", RpcTarget.All);
                //UIController.instance.passButton.SetActive(IsMyTurn());
                //photonView.RPC("EndTurnPlayer", RpcTarget.All);
                //GameManager.instace.state = GameManager.States.SWITCH_PLAYER;
            }

            //purple1 route
            if (routePosition % fullRoute.Count == 4)
            {
                Debug.Log("in purple 1 route");
                photonView.RPC("valueUpdate", RpcTarget.All);

                //test if donate
                photonView.RPC("ShowDonateCardToAllPlayerRPC", RpcTarget.All);


                //GameManager.instace.state = GameManager.States.SWITCH_PLAYER;
            }
            //purple2 route
            if (routePosition % fullRoute.Count == 12)
            {
                Debug.Log("in purple 2 route");
                photonView.RPC("valueUpdate", RpcTarget.All);
                //UIController.instance.passButton.SetActive(IsMyTurn());
                if (((GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].getmoney * 10) - GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].loanBank) > 0
                    && ((GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].getmoney * 10) - GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].loanBank) >= GameManager.instace.playerList[GameManager.instace.activePlayer].paid
                    && GameManager.instace.playerList[GameManager.instace.activePlayer].money < GameManager.instace.playerList[GameManager.instace.activePlayer].paid)
                {
                    //auto loan
                    GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money += GameManager.instace.playerList[GameManager.instace.activePlayer].paid;
                    GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].loanBank += GameManager.instace.playerList[GameManager.instace.activePlayer].paid;
                    photonView.RPC("UpdatePayLoan", RpcTarget.All, GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money, GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].loanBank);
                    photonView.RPC("UpdateLoanMoney", RpcTarget.All, GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money, PhotonNetwork.LocalPlayer.ActorNumber - 1, GameManager.instace.playerList[GameManager.instace.activePlayer].paid);
                    photonView.RPC("valueUpdate", RpcTarget.All);
                }
                else if (GameManager.instace.playerList[GameManager.instace.activePlayer].getmoney <= 0 && GameManager.instace.playerList[GameManager.instace.activePlayer].money < GameManager.instace.playerList[GameManager.instace.activePlayer].paid)
                {
                    //lose
                    GameManager.instace.playerList[GameManager.instace.activePlayer].playerType = GameManager.Entity.PlayerTypes.NO_PLAYER;
                    GameManager.instace.playerInRoom = 0;
                    for (int i = 0; i < GameManager.instace.playerList.Count; i++)
                    {
                        if (GameManager.instace.playerList[i].playerType == GameManager.Entity.PlayerTypes.HUMAN)
                        {
                            GameManager.instace.playerInRoom++;
                        }

                    }
                    if (GameManager.instace.playerInRoom > 0)
                    {
                        GameManager.instace.state = GameManager.States.SWITCH_PLAYER;
                    }
                    UIController.instance.BlurBg.SetActive(true);
                    UIController.instance.lostShow.SetActive(true);
                }
                GameManager.instace.playerList[GameManager.instace.activePlayer].money -= GameManager.instace.playerList[GameManager.instace.activePlayer].paid;
                photonView.RPC("UpdateMoneyNoJob", RpcTarget.All, GameManager.instace.playerList[GameManager.instace.activePlayer].money, GameManager.instace.activePlayer);
                GameManager.instace.playerList[GameManager.instace.activePlayer].hasDonate = false;
                GameManager.instace.playerList[GameManager.instace.activePlayer].hasDonateCount = 0;
                photonView.RPC("setDonate", RpcTarget.All, GameManager.instace.playerList[GameManager.instace.activePlayer].hasDonate, GameManager.instace.playerList[GameManager.instace.activePlayer].hasDonateCount);
                photonView.RPC("hasNoJob", RpcTarget.All);
                photonView.RPC("valueUpdate", RpcTarget.All);
                
                
                photonView.RPC("EndTurnPlayer", RpcTarget.All);


            }
            //purple3 route
            if (routePosition % fullRoute.Count == 20)
            {
                Debug.Log("in purple 3 route");
                photonView.RPC("GetChild", RpcTarget.All);
                photonView.RPC("valueUpdate", RpcTarget.All);
                photonView.RPC("EndTurnPlayer", RpcTarget.All);

            }
            //green route
            if (routePosition % 2 == 1)
            {
                Debug.Log("in green route");
                photonView.RPC("setUpDeckSmallDealToEveryone", RpcTarget.All);
                photonView.RPC("setUpDeckBigDealToEveryone", RpcTarget.All);
                //UIController.instance.passButton.SetActive(IsMyTurn());
                photonView.RPC("valueUpdate", RpcTarget.All);
                photonView.RPC("PlayerChooseSmallBig", RpcTarget.All);
                //GameManager.instace.state = GameManager.States.SWITCH_PLAYER;
            }/*
            else
            {
                Debug.Log("in blue route");
                remiderPosition = false;
                photonView.RPC("valueUpdate", RpcTarget.All);
                Debug.Log(routePosition % fullRoute.Count + " " + steps + " " + routePosition + " " + isMoving + " " + doneSteps);
                photonView.RPC("PlayerMarketDraw", RpcTarget.All);
            }*/

        }


    }

    bool MoveInArcToNextNode(Vector3 startPos, Vector3 lastPos, float speed)
    {
        cTime += speed * Time.deltaTime;
        Vector3 myPos = Vector3.Lerp(startPos, lastPos, cTime);

        myPos.y += amplitude * Mathf.Sin(Mathf.Clamp01(cTime) * Mathf.PI);

        return lastPos != (transform.position = Vector3.Lerp(transform.position, myPos, cTime));
    }




    public bool ReturnIsOut()
    {
        return isOut;
    }

    public void leaveBase()
    {
        steps = 0;
        isOut = true;
        routePosition = 0;
        //start coroutine

        //StartCoroutine(MoveOut());
    }





    public void StartTheMove(int DiceNumber)
    {

        steps = DiceNumber;
        photonView.RPC("MovePlayer", RpcTarget.All);

    }

    //wait change
    bool winCondition()
    {
        for (int i = 0; i < outerRoute.childNodeList.Count; i++)
        {
            if (!outerRoute.childNodeList[i].GetComponent<Node>().isTaken)
            {
                return false;
            }
        }
        return true;
    }

    //---------------------------------Human input------------------------------
    public void SetSelector(bool on)
    {
        if (IsMyTurn())
        {
            hasTurn = on;
            photonView.RPC("hasTurnToEveryone", RpcTarget.All, on);

        }
    }

    public void tohasturn()
    {
        if (GameManager.instace.playerList[GameManager.instace.activePlayer].hasTurn)
        {

            if (GameManager.instace.playerList[GameManager.instace.activePlayer].isClick2Dice == true)
            {
                StartCoroutine(WaitForDiceRoll());
            }
            else
            {
                GameManager.instace.rolledhumanDice = GameManager.instace.dice2.diceValue + GameManager.instace.dice.diceValue;
                StartTheMove(GameManager.instace.rolledhumanDice);

            }

        }

        GameManager.instace.DeactivateAllSelector();
    }


    IEnumerator WaitForDiceRoll()
    {
        while (GameManager.instace.dice.diceValue == 0 || GameManager.instace.dice2.diceValue == 0)
        {
            yield return null; // Wait for the next frame
        }

        GameManager.instace.rolledhumanDice = GameManager.instace.dice2.diceValue + GameManager.instace.dice.diceValue;
        StartTheMove(GameManager.instace.rolledhumanDice);
        GameManager.instace.playerList[GameManager.instace.activePlayer].isClick2Dice = false;
        photonView.RPC("CheckClick2Dice", RpcTarget.All, GameManager.instace.playerList[GameManager.instace.activePlayer].isClick2Dice);


    }

    private bool IsMyTurn()
    {
        // Replace with your logic. This could be checking against a player list, an ID, etc.
        return GameManager.instace.activePlayer == PhotonNetwork.LocalPlayer.ActorNumber - 1;
    }

    [PunRPC]
    void CheckClick2Dice(bool is2dice)
    {
        GameManager.instace.playerList[GameManager.instace.activePlayer].isClick2Dice = is2dice;
    }

    [PunRPC]
    void MovingPlayer(int diceNum)
    {
        StartTheMove(diceNum);
    }

    [PunRPC]
    void hasNoJob()
    {
        GameManager.instace.playerList[GameManager.instace.activePlayer].hasJobCount = 2;
    }

    [PunRPC]
    void GetChild()
    {
        GameManager.instace.playerList[GameManager.instace.activePlayer].hasChild = true;
        if (GameManager.instace.playerList[GameManager.instace.activePlayer].child <= 3)
        {
            GameManager.instace.playerList[GameManager.instace.activePlayer].child += 1;
        }

    }

    [PunRPC]
    void hasTurnToEveryone(bool on)
    {
        GameManager.instace.playerList[GameManager.instace.activePlayer].hasTurn = on;
    }

    [PunRPC]
    void MovePlayer()
    {
        Debug.Log(Move());

        StartCoroutine(Move());
        //photonView.RPC("Move1", RpcTarget.All);
    }

    [PunRPC]
    void EndTurnPlayer()
    {
        UIController.instance.passButton.SetActive(IsMyTurn());

    }

    [PunRPC]
    void PlayerDraw()
    {
        GameManager.instace.playerList[GameManager.instace.activePlayer].isInRedRoute = true;
        GameManager.instace.playerList[GameManager.instace.activePlayer].isDrawButtonOn = true;
        UIController.instance.drawButton.SetActive(IsMyTurn());
    }

    [PunRPC]
    void PlayerMarketDraw()
    {
        Debug.Log(routePosition % fullRoute.Count + " " + steps + " " + routePosition + " " + isMoving + " " + doneSteps);
        UIController.instance.MarketDrawButton.SetActive(IsMyTurn());
    }

    [PunRPC]
    void PlayerChooseSmallBig()
    {
        UIController.instance.ChooseBigSmall.SetActive(IsMyTurn());
    }

    [PunRPC]
    void UpdateMoney(int money, int x)
    {
        GameManager.instace.playerList[x].money = money;
        UIController.instance.MyMoneyText.text = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money.ToString();
        //note collect
        GameManager.Note myNote = new GameManager.Note();
        myNote.CardName = "+ " + "Month Income";
        myNote.price = GameManager.instace.playerList[GameManager.instace.activePlayer].getmoney;
        GameManager.instace.playerList[GameManager.instace.activePlayer].Keep.Add(myNote);

        GameManager.Note myNote2 = new GameManager.Note();
        myNote2.CardName = "= ";
        myNote2.price = GameManager.instace.playerList[x].money;
        GameManager.instace.playerList[x].Keep.Add(myNote2);
    }

    [PunRPC]
    void UpdateMoneyNoJob(int money, int x)
    {
        GameManager.instace.playerList[x].money = money;
        UIController.instance.MyMoneyText.text = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money.ToString();
        //note collect
        GameManager.Note myNote = new GameManager.Note();
        myNote.CardName = "- " + "unemployed";
        myNote.price = GameManager.instace.playerList[x].paid;
        GameManager.instace.playerList[GameManager.instace.activePlayer].Keep.Add(myNote);

        GameManager.Note myNote2 = new GameManager.Note();
        myNote2.CardName = "= ";
        myNote2.price = GameManager.instace.playerList[x].money;
        GameManager.instace.playerList[x].Keep.Add(myNote2);
    }

    [PunRPC]
    void UpdateMoneyDonate(int money, int x)
    {
        GameManager.instace.playerList[x].money = money;
        UIController.instance.MyMoneyText.text = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money.ToString();
        //note collect
        GameManager.Note myNote = new GameManager.Note();
        myNote.CardName = "- " + "Donate";
        myNote.price = GameManager.instace.playerList[GameManager.instace.activePlayer].allRecieve / 10;
        GameManager.instace.playerList[GameManager.instace.activePlayer].Keep.Add(myNote);

        GameManager.Note myNote2 = new GameManager.Note();
        myNote2.CardName = "= ";
        myNote2.price = GameManager.instace.playerList[x].money;
        GameManager.instace.playerList[x].Keep.Add(myNote2);

    }
    [PunRPC]
    void valueUpdate()
    {
        UIController.instance.MyMoneyText.text = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money.ToString();
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].allRecieve = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].salary + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].income;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].InstallmentsBank = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].loanBank / 10;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].sumChild = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].child * GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].perChild;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].paid = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].tax + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].homeMortgage + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].learnMortgage + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].carMortgage + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].creditcardMortgage + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].extraPay + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].InstallmentsBank + GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].sumChild;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].getmoney = GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].allRecieve - GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].paid;
    }

    [PunRPC]
    void setDonate(bool isDonate, int count)
    {
        GameManager.instace.playerList[GameManager.instace.activePlayer].hasDonate = isDonate;
        GameManager.instace.playerList[GameManager.instace.activePlayer].hasDonateCount = count;
    }

    [PunRPC]
    void ShowDonateCardToAllPlayerRPC()
    {
        UIController.instance.cardShow.enabled = true;
        UIController.instance.cardShow.sprite = UIController.instance.DonateCard;
        UIController.instance.PayDonateBtn.SetActive(IsMyTurn());
        UIController.instance.cancelButton.SetActive(IsMyTurn());
    }

    [PunRPC]
    void setUpDeckMarketToEveryone()
    {
        if (MarketDeckController.instance.activeCards.Count <= 1)
        {
            MarketDeckController.instance.SetUpDeck();
        }
    }

    [PunRPC]
    void setUpDeckSmallDealToEveryone()
    {
        if (SmallDealDeckController.instance.activeCards.Count <= 1)
        {
            SmallDealDeckController.instance.SetUpDeck();
        }
    }

    [PunRPC]
    void setUpDeckBigDealToEveryone()
    {
        if (BigDealDeckController.instance.activeCards.Count <= 1)
        {
            BigDealDeckController.instance.SetUpDeck();
        }
    }

    [PunRPC]
    void setUpDeckSpendToEveryone()
    {
        if (SpendDeckController.instance.activeCards.Count <= 1)
        {
            SpendDeckController.instance.SetUpDeck();
        }
    }


    [PunRPC]
    void UpdatePayLoan(int money,int loanBank)
    {
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].money = money;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].loanBank = loanBank;
    }


    [PunRPC]
    void UpdateLoanMoney(int money, int x, int loan)
    {
        GameManager.instace.playerList[x].money = money;
        //note collect
        GameManager.Note myNote = new GameManager.Note();
        myNote.CardName = "+ " + " Loan";
        myNote.price = loan;
        GameManager.instace.playerList[PhotonNetwork.LocalPlayer.ActorNumber - 1].Keep.Add(myNote);

        GameManager.Note myNote2 = new GameManager.Note();
        myNote2.CardName = "= ";
        myNote2.price = GameManager.instace.playerList[x].money;
        GameManager.instace.playerList[x].Keep.Add(myNote2);

    }
}


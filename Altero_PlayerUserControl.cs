/* 
	Script: Altero_PlayerUserControl.cs
	Author: Sergio Schiavo
	Project: Altero
	
 	Description: 
	This is a class from my upcoming game called Altero (it is still under development, so this class still needs to be optmized). 
	
	This class is responsible for giving the player gameobject its main functionality:
	
	 - Making the character move
	 - Saving the players action (eg: moving forward, staying idle, jump, etc)
	 - Spawning a "copy" of the player, and re-playing his actions
	
	
 */


using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using InControl;

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof(PlatformerCharacter2D))]
    public class Altero_PlayerUserControl : MonoBehaviour
    {

        public int PlayerNum = 1;

        private PlatformerCharacter2D m_Character;
        private bool m_Jump;

        [HideInInspector]
        public List<string> ActionList = new List<string>();

        [HideInInspector]
        public string currentAction = "Idle";

        [HideInInspector]
        public bool IsGhost = false;
        float startTime;

        //PlayerVARS
        Vector3 BornPosition;
        GameObject UsableOject;
        srcGameController gameController;

        private Text txtPlayerPos;

        //GhostVARS
        [HideInInspector]
        public float timer = 0;
        [HideInInspector]
        public string CurrentAction = "Idle";
        int CurrentActionNumber = 0;
        [HideInInspector]
        public float CurrentActionTimer = 0;

        bool destroyItself = false;
        float destroyCountDown = 10f;

        [HideInInspector]
        public bool LookUp = false;
        [HideInInspector]
        public bool LookDown = false;

        [HideInInspector]
        public float h = 0;
        private float acelSpeed = 3;
        public TextMesh txt_h;

        private bool Moving = true;



        private InputDevice device;

        private void Awake()
        {
            m_Character = GetComponent<PlatformerCharacter2D>();
        }
        void Start()
        {
            txt_h.text = "";
            BornPosition = this.gameObject.transform.position;
            setGameController();

            if (!IsGhost)
            {
                GameObject playerposObj = GameObject.Find("txtPlayerPos");
                if (playerposObj != null)
                    txtPlayerPos = playerposObj.GetComponent<Text>();
            }

            //remove after
            //setGhostUp(); 
        }


        void setGameController()
        {
            GameObject gameControllerObj = GameObject.FindGameObjectWithTag("GameController");
            if (gameControllerObj != null) gameController = gameControllerObj.GetComponent<srcGameController>();
            startTime = Time.fixedTime;
        }


        void StartPlayer()
        {
        }

        private void CheckAndAssignDevice()
        {
            if (device != null) return;

            else
            {
                if (PlayerNum == 2)
                {
                    if (InputManager.Devices.Count > 1) device = InputManager.Devices[1];
                }
                else
                {
                    if (InputManager.Devices.Count > 0) device = InputManager.Devices[0];
                }
            }
        }

        public void StayIdle()
        {
            LookDown = false;
            LookUp = false;
            h = 0;

            m_Character.Move(h, LookDown, LookUp, m_Jump);
            m_Character.resetVSpeed();

        }

        private void FixedUpdate()
        {
            Moving = false;

            if(gameController.DebugMode) if (txt_h != null) txt_h.text = h.ToString();

            if (gameController.gamePaused) return;

            CheckAndAssignDevice();

            if (IsGhost)
            {
                timer = (Time.fixedTime - startTime);
                if (timer >= CurrentActionTimer) MoveToNextAction();
            }
            else
            {
                
                //Joystick Moviments (any player)
                if (device != null)
                {


                    LookDown = (device.LeftStickY.Value <= -0.6) || (device.DPadY.Value <= -1);
                    LookUp = (device.LeftStickY.Value >= 0.6) || (device.DPadY >= 1);

                    if (device.LeftStickX.Value != 0)
                    {
                        if (device.LeftStickX.Value > 0.6f) MoveRight();
                        else if (device.LeftStickX.Value < -0.6) MoveLeft();
                    }
                    else if (device.DPadX.Value != 0)
                    {
                        if (device.DPadX.Value > 0.6f) MoveRight();
                        else if (device.DPadX.Value < -0.6f) MoveLeft();
                    }
                }

                ////Keyboard Moviments
                if (PlayerNum == 1)
                {
                    LookDown = (Input.GetKey(KeyCode.DownArrow));
                    LookUp = (Input.GetKey(KeyCode.UpArrow));
                    if (Input.GetKey(KeyCode.LeftArrow)) MoveLeft();
                    else if (Input.GetKey(KeyCode.RightArrow)) MoveRight();

                }
                else if (PlayerNum == 2)
                {
                    LookDown = (Input.GetKey(KeyCode.S));
                    LookUp = (Input.GetKey(KeyCode.W));
                    if (Input.GetKey(KeyCode.A)) h = -1;
                    else if (Input.GetKey(KeyCode.D)) h = 1;
                }


                if (!Moving) DontMove();

                NewSaveAction();
            }
            m_Character.Move(h, LookDown, LookUp, m_Jump);
            m_Jump = false;
        }

        private void Update()
        {
            if (gameController.gamePaused) return;

            if (!IsGhost)
            {
                if (txtPlayerPos != null) txtPlayerPos.text = Mathf.RoundToInt(transform.position.x).ToString() + "-" + Mathf.RoundToInt(transform.position.y).ToString();

                //Manage Jump (keyboard or joystick)
                if (!m_Jump)
                {
                    if (device != null) m_Jump = (device.Action1.WasPressed);
                    bool jumpKeyboard = false;
                    if (PlayerNum == 1) jumpKeyboard = Input.GetKeyDown(KeyCode.Space);
                    else if (PlayerNum == 2) jumpKeyboard = Input.GetKeyDown(KeyCode.J);
                    if (jumpKeyboard) m_Jump = true;
                }

                //Manage Use (keyboard or joystick)
                if (device != null) if (device.Action3.WasPressed) Use();
                if (PlayerNum == 1 && Input.GetKeyDown(KeyCode.LeftControl)) Use();
                else if (PlayerNum == 2 && Input.GetKeyDown(KeyCode.U)) Use();
            }

            if (destroyItself)
            {
                destroyCountDown -= Time.deltaTime;
                if (destroyCountDown <= 0) DestroyMe();
            }
        }

        void Use()
        {
            if (!IsGhost) ActionList.Add(h.ToString() + ";Use");
            if (UsableOject != null)
            {

                switch (UsableOject.tag.ToLower())
                {
                    case "leveldoor": UsableOject.GetComponent<srcDoor>().ActivateItem(); break;
                    case "checkpoint": UsableOject.GetComponent<srcCheckPoint>().ActivateItem(); break;
                    default:
                        if (UsableOject.GetComponent<srcButton>() != null) UsableOject.GetComponent<srcButton>().ActivateItem();
                        else UnSetUsableObject();
                        break;
                }
            }
        }

        public void SetUsableObject(GameObject _usableObject)
        {
            UsableOject = _usableObject;
        }

        public GameObject GetUsableObject()
        {
            return UsableOject;
        }

        public void UnSetUsableObject()
        {
            UsableOject = null;
        }

        void OnCollisionEnter2D(Collision2D Col)
        {
            if (Col.collider.tag == "Enemy" || Col.collider.tag == "Blade")
            {
                if (!IsGhost)
                {
                    gameController.LoseLife(ActionList, BornPosition, transform.position);
                }
                DestroyMe();
            }
        }

        public void DestroyMe()
        {
            gameController.DeathDust(this.transform.position);
            Destroy(this.gameObject);
        }

        public void SetCheckPoint(Vector3 NewBornPosition)
        {
            if (!IsGhost)
            {
                ActionList.Clear();
                startTime = Time.fixedTime;
                BornPosition = this.transform.position;
            }
        }

        private void NewSaveAction()
        {
            string doJump = "", doUse = "";
            if (m_Jump) doJump = ";j";
            ActionList.Add(h.ToString() + doJump + doUse);
        }



        //GHOST METHODS
        void setGhostUp()
        {
            if (gameController == null) setGameController();

            Anima2D.SpriteMeshInstance head = transform.Find("Player/Meshes/head").gameObject.GetComponent<Anima2D.SpriteMeshInstance>();
            Anima2D.SpriteMeshInstance head_back = transform.Find("Player/Meshes/head_back").gameObject.GetComponent<Anima2D.SpriteMeshInstance>();
            Anima2D.SpriteMeshInstance leg_back = transform.Find("Player/Meshes/leg_back").gameObject.GetComponent<Anima2D.SpriteMeshInstance>();
            Anima2D.SpriteMeshInstance leg_front = transform.Find("Player/Meshes/leg_front").gameObject.GetComponent<Anima2D.SpriteMeshInstance>();
            Anima2D.SpriteMeshInstance arm_back = transform.Find("Player/Meshes/arm_back").gameObject.GetComponent<Anima2D.SpriteMeshInstance>();
            Anima2D.SpriteMeshInstance arm_front = transform.Find("Player/Meshes/arm_front").gameObject.GetComponent<Anima2D.SpriteMeshInstance>();
            Anima2D.SpriteMeshInstance body = transform.Find("Player/Meshes/body").gameObject.GetComponent<Anima2D.SpriteMeshInstance>();
            Anima2D.SpriteMeshInstance eye_left = transform.Find("Player/Meshes/eye_left").gameObject.GetComponent<Anima2D.SpriteMeshInstance>();
            Anima2D.SpriteMeshInstance eye_right = transform.Find("Player/Meshes/eye_right").gameObject.GetComponent<Anima2D.SpriteMeshInstance>();

            Color ghostColor = new Color(0.434f, 0.434f, 0.434f, 1.000f);
            int ghostSortingLayer = SortingLayer.NameToID("PlayerGhost");
            int orderMult = gameController.getGhostCounter();

            head.setGhostSettings(ghostColor, ghostSortingLayer, orderMult);
            head_back.setGhostSettings(ghostColor, ghostSortingLayer, orderMult);
            leg_back.setGhostSettings(ghostColor, ghostSortingLayer, orderMult);
            leg_front.setGhostSettings(ghostColor, ghostSortingLayer, orderMult);
            arm_back.setGhostSettings(ghostColor, ghostSortingLayer, orderMult);
            arm_front.setGhostSettings(ghostColor, ghostSortingLayer, orderMult);
            body.setGhostSettings(ghostColor, ghostSortingLayer, orderMult);
            eye_left.setGhostSettings(ghostColor, ghostSortingLayer, orderMult);
            eye_right.setGhostSettings(ghostColor, ghostSortingLayer, orderMult);
        }

        void TheStart(List<string> _ActionList)
        {
            IsGhost = true;
            gameObject.tag = "PlayerGhost";
            setGhostUp();
            ActionList = _ActionList;
        }

        void MoveToNextAction()
        {
            CurrentActionNumber++;
            if (CurrentActionNumber >= ActionList.Count - 1)
            {
                destroyItself = true;
            }
            else
            {
                string[] NextAction = ActionList[CurrentActionNumber].Split(';');
                for (int i = 0; i < NextAction.Length; i++)
                {
                    if(i==0) h = float.Parse(NextAction[i]);
                    else
                    {
                        if (NextAction[i] == "j") m_Jump = true;
                        else if (NextAction[i] == "Use") Use();
                    }
                }
            }
        }
		
		private void DontMove()
        {
            if (!m_Character.m_Grounded) return;
            if (h > 0)
            {
                h = h - Time.deltaTime * (acelSpeed * 2);
                if (h < 0) h = 0;
            }
            else if (h < 0)
            {
                h = h + Time.deltaTime * (acelSpeed * 2);
                if (h > 0) h = 0;
            }
        }

        private void MoveRight()
        {
            Moving = true;
            if (h < 0) h = 0;
            else if (h >= 1) h = 1;
            else
            {
                h = h + Time.deltaTime * acelSpeed;
                if (h >= 1) h = 1;
            }
        }

        private void MoveLeft()
        {
            Moving = true;
            if (h > 0) h = 0;
            else if (h <= -1) h = -1;
            else
            {
                h = h - Time.deltaTime * acelSpeed;
                if (h <= -1) h = -1;
            }
        }
    }
}

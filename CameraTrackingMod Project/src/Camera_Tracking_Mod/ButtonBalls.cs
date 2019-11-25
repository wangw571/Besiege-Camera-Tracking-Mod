using System;
using System.Collections.Generic;
using Modding;
using UnityEngine;


namespace forceUnityTools
{
    public static class ButtonBalls 
    {
        //public ButtonBalls()
        //{
        //    GameObject go = new GameObject();
        //}
        static ChewChew yumyumyum;
        public delegate void UpdateEvent();
        public static Vector3 RatioApply(Rect Screen, Vector3 Ratio)
        {
            return new Vector3(Screen.width * Ratio.x / 100, Screen.height * Ratio.y / 100, Ratio.z);
        }

        static ClickButtonBall ShowUpAlll;
        static KeyAssignButtonBall TriggerSetter;
        static KeyAssignButtonBall TriggerOffSetter;
        static SliderButtonBall ActivationRange;

        public static void INNIT(ChewChew yumyum)
        {
            GameObject broadCast = new GameObject("MousePosBroadCaster");
            MouseRayBroadCaster 广播者 = broadCast.AddComponent<MouseRayBroadCaster>();
            yumyumyum = yumyum;
            GameObject ShowUpAll = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            StatMaster.DontDestroyOnLoad(ShowUpAll);
            ShowUpAlll = ShowUpAll.AddComponent<ClickButtonBall>();
            UpdateEvent EUSUA = new UpdateEvent(ShowThemAll);
            ShowUpAlll.SetUp(Color.red, Color.white, true, false, "Show Camera \n Tracking Menu", EUSUA, new Vector3(0, 40, 35), new Vector3(0, 40, -15 ));
            ShowUpAlll.Show();
            广播者.AddMeIn(ShowUpAlll);



            GameObject SetTriggerKey = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatMaster.DontDestroyOnLoad(SetTriggerKey);
            TriggerSetter = SetTriggerKey.AddComponent<KeyAssignButtonBall>();
            UpdateEvent EUTS = new UpdateEvent(UpdateKey);
            TriggerSetter.SetUp(Color.cyan, Color.red, KeyCode.RightAlt, "Key For \nTracking: {0}", EUTS, new Vector3(20, 40, 15), new Vector3(120, 120, -150));
            广播者.AddMeIn(TriggerSetter);


            GameObject SetOffKey = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatMaster.DontDestroyOnLoad(SetOffKey);
            TriggerOffSetter = SetOffKey.AddComponent<KeyAssignButtonBall>();
            UpdateEvent EUTOS = new UpdateEvent(UpdateOffKey);
            TriggerOffSetter.SetUp(Color.cyan, Color.red, KeyCode.Mouse1, "Key For Disable\nTracking: {0}", EUTOS, new Vector3(40, 40, 15), new Vector3(120, 130, -150));
            广播者.AddMeIn(TriggerOffSetter);



            GameObject ActivationRangeBall = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            StatMaster.DontDestroyOnLoad(ActivationRangeBall);
            ActivationRange = ActivationRangeBall.AddComponent<SliderButtonBall>();
            UpdateEvent EUAR = new UpdateEvent(UpdateActivationRange);
            ActivationRange.SetUp(1, 100, 35, 0.1f, "Track when target \nat {0}% on the screen", EUAR, new Vector3(15, 60, 25), new Vector3(120, 140, -150));
            广播者.AddMeIn(ActivationRange);
        }

        static void ShowThemAll()
        {
            if (ShowUpAlll.SwitchBool)
            {
                TriggerSetter.Show();
                ActivationRange.Show();
                TriggerOffSetter.Show();
            }
            else
            {
                TriggerSetter.Hide();
                ActivationRange.Hide();
                TriggerOffSetter.Hide();
            }
        }

        static void UpdateKey()
        {
            yumyumyum.activation = TriggerSetter.toAssign;
        }
        static void UpdateOffKey()
        {
            yumyumyum.triggerOffKey = TriggerOffSetter.toAssign;
        }

        static void UpdateActivationRange()
        {
            yumyumyum.ActivationRange = ActivationRange.Value / 100;
        }
    }

    public abstract class AbsButtonBall : MonoBehaviour
    {
        protected bool ShowUp;
        public string Description;
        public Vector3 ShowLocationRatio = new Vector2(11.4f, 51.4f);
        public Vector3 HideLocationRatio = new Vector2(1919, 810);
        public ButtonBalls.UpdateEvent updateEvent;
        public GameObject myText;
        protected TextMesh myTextMesh;
        protected Renderer MeeRee;
        protected Collider Mee;
        public abstract void MouseRelatedUpdate(RaycastHit[] hits);
        protected abstract void MyUpdate();
        void MyStart()
        {
            MeeRee = this.GetComponent<Renderer>();
            //Texture ttx = MeeRee.material.mainTexture;
            //MeeRee.material.shader = Shader.Find("FX/Glass/Stained BumpDistort");
            //MeeRee.material.mainTexture = ttx;
            MeeRee.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Mee = this.GetComponent<Collider>();
            Mee.isTrigger = true;
        }
        public void Show()
        {
            ShowUp = true;
        }
        public void Hide()
        {
            ShowUp = false;
        }
        void Start()
        {
            myText = new GameObject("myDescription");
            myText.transform.parent = this.transform;
            myText.transform.localPosition = Vector3.zero;
            myText.transform.localScale = new Vector3(-0.05f, 0.05f, 0.05f);
            myTextMesh = myText.AddComponent<TextMesh>();
            myTextMesh.fontSize = 72;
            myTextMesh.color = Color.black;
            myTextMesh.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf"); ;
            myText.AddComponent<AutoLookAt>();
            MyStart();
        }
        void Update()
        {
            //this.transform.position = Vector3.Lerp(
            //    this.transform.position,
            //    Camera.main.ScreenToWorldPoint(ButtonBalls.RatioApply(Camera.main.pixelRect, ShowUp ? ShowLocationRatio : HideLocationRatio)),
            //    1f
            //    );

            bool isBuildingMode = ((StatMaster.SimulationState == SimulationState.SpectatorMode) ||
                (StatMaster.SimulationState == SimulationState.BuildMode) ||
                (StatMaster.SimulationState == SimulationState.BuildModeGlobalSim) ||
                (StatMaster.SimulationState == SimulationState.BuildModeGlobalSimRemoteLocalSim) ||
                (StatMaster.SimulationState == SimulationState.BuildModeRemoteLocalSim));

            this.transform.position = Vector3.Lerp(this.transform.position, 
                Camera.main.ScreenToWorldPoint(ButtonBalls.RatioApply(Camera.main.pixelRect, ShowUp && isBuildingMode ? ShowLocationRatio : HideLocationRatio)), 0.1f);

            this.transform.localScale = Vector3.Lerp(this.transform.localScale, ShowUp && isBuildingMode ? Vector3.one : Vector3.zero, 0.1f);
            MyUpdate();
        }
        void FixedUpdate()
        {
            bool isBuildingMode = ((StatMaster.SimulationState == SimulationState.SpectatorMode) ||
                (StatMaster.SimulationState == SimulationState.BuildMode) ||
                (StatMaster.SimulationState == SimulationState.BuildModeGlobalSim) ||
                (StatMaster.SimulationState == SimulationState.BuildModeGlobalSimRemoteLocalSim) ||
                (StatMaster.SimulationState == SimulationState.BuildModeRemoteLocalSim));
            this.transform.position = Vector3.Lerp(this.transform.position,
                 Camera.main.ScreenToWorldPoint(ButtonBalls.RatioApply(Camera.main.pixelRect, ShowUp && isBuildingMode ? ShowLocationRatio : HideLocationRatio)), 0.1f);
            this.transform.localScale = Vector3.Lerp(this.transform.localScale, ShowUp && isBuildingMode ? Vector3.one : Vector3.zero, 0.1f);
        }
    }
    public class ClickButtonBall : AbsButtonBall
    {
        public Color OnColor;
        public Color OffColor;
        public bool isSwitch;
        public bool SwitchBool;

        public override void MouseRelatedUpdate(RaycastHit[] hits)
        {

            foreach (RaycastHit hitt in hits)
            {
                if (hitt.collider == Mee)
                {
                    MeeRee.material.color = OnColor;
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        if (isSwitch)
                        {
                            SwitchBool = !SwitchBool;
                        }
                        updateEvent();
                        
                    }

                    break;
                }
                else
                {
                    MeeRee.material.color = OffColor;                    
                }
            }
            if (isSwitch)
            {
                MeeRee.material.color = SwitchBool ? OnColor : OffColor;
            }
        }

        public void SetUp(Color OnColorR, Color AwayColorR, bool isSwitchh, bool defSwitchBool, String text, ButtonBalls.UpdateEvent toDo, Vector3 ShowRatio, Vector3 HideRatio)
        {
            OnColor = OnColorR;
            OffColor = AwayColorR;
            isSwitch = isSwitchh;
            SwitchBool = defSwitchBool;
            Description = text;
            updateEvent = toDo;
            ShowLocationRatio = ShowRatio;
            HideLocationRatio = HideRatio;
        }

        protected override void MyUpdate() 
        {
            myTextMesh.text = string.Format(Description, SwitchBool);
        }

    }

    public class KeyAssignButtonBall : AbsButtonBall
    {
        public Color OnColor;
        public Color AwayColor;
        public KeyCode toAssign;
        

        public void SetUp(Color OnColorR, Color AwayColorR, KeyCode DefaultKey, String text, ButtonBalls.UpdateEvent toDo, Vector3 ShowRatio, Vector3 HideRatio)
        {
            OnColor = OnColorR;
            AwayColor = AwayColorR;
            toAssign = DefaultKey;
            Description = text;
            updateEvent = toDo;
            ShowLocationRatio = ShowRatio;
            ShowLocationRatio.z = 25;
            HideLocationRatio = HideRatio;
            HideLocationRatio.z = -1;
        }

        public override void MouseRelatedUpdate(RaycastHit[] hits)
        {
            foreach(RaycastHit hitt in hits)
            {
                if (hitt.collider == Mee)
                {
                    MeeRee.material.color = OnColor;
                    if (Input.anyKey)
                        toAssign = Event.current.keyCode == KeyCode.None? toAssign : Event.current.keyCode;

                    updateEvent();
                    break;
                }
                else
                {
                    MeeRee.material.color = AwayColor;
                }
            }
        }

        protected override void MyUpdate() 
        {
            myTextMesh.text = string.Format(Description, toAssign.ToString());
        }
    }

    public class SliderButtonBall : AbsButtonBall
    {
        bool MouseDraggingPrev = false;
        bool OnMe = false;
        Vector3 MousePosPrev = Vector3.zero;
        public float Min;
        public float Max;
        public float Value;
        public float DragRatio;

        public void SetUp(float min, float max, float defValue, float DragRatioo, String text, ButtonBalls.UpdateEvent toDo, Vector3 ShowRatio, Vector3 HideRatio)
        {
            Min = min;
            Max = max;
            Value = defValue;
            DragRatio = DragRatioo;
            Description = text;
            updateEvent = toDo;
            ShowLocationRatio = ShowRatio;
            HideLocationRatio = HideRatio;
            this.gameObject.AddComponent<AutoLookAt>();
        }
        public override void MouseRelatedUpdate(RaycastHit[] hits)
        {
            foreach (RaycastHit hitt in hits)
            {
                OnMe = hitt.collider == Mee;
                if (OnMe) break;
            }
        }

        protected override void MyUpdate()
        {
            if (Input.GetKey(KeyCode.Mouse0) && (OnMe|| MouseDraggingPrev))
            {
                if (MouseDraggingPrev)
                {
                    float diff = Vector3.Dot((Input.mousePosition - MousePosPrev), new Vector3(1, 1, 0));
                    Value += diff* DragRatio;
                    Value = Mathf.Clamp(Value, Min, Max);
                }
                MousePosPrev = Input.mousePosition;
                MouseDraggingPrev = true;

                updateEvent();
            }
            else
            {
                MouseDraggingPrev = false;
            }
            myTextMesh.text = string.Format(Description, Value);
        }

    }



    public class MouseRayBroadCaster : MonoBehaviour
    {
        public List<AbsButtonBall> buttonBalls = new List<AbsButtonBall>();
        public void AddMeIn(AbsButtonBall oneButtonBall)
        {
            buttonBalls.Add(oneButtonBall);
        }

        void Update()
        {
            RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 720)));
            foreach(AbsButtonBall bb in buttonBalls)
            {
                bb.MouseRelatedUpdate(hits);
            }
        }
    }

    public class AutoLookAt:MonoBehaviour
    {
        void Update()
        {
            this.transform.LookAt(Camera.main.transform);
        }
    }


}

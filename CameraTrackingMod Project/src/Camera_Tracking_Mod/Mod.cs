using System;
using System.Collections.Generic;
using Modding;
using UnityEngine;

namespace forceUnityTools
{
    public class Mod : ModEntryPoint
    {
        public override void OnLoad()
        {

            GameObject go = new GameObject("Camera Tracking Mod");
            ChewChew chw = go.AddComponent<ChewChew>();
            StatMaster.DontDestroyOnLoad(go);
            ButtonBalls.INNIT(chw);
        }
    }

    public class ChewChew : MonoBehaviour
    {
        private float defaultFoV = 114514;
        public MouseOrbit theMouseOrbit;
        public Camera mouseOrbitMainCam;
        public bool basedOnTime;
        public bool Enabllled = false;
        //private bool PreWasShrinking = false;
        public Transform CurrentTarget;
        public Vector3 CurrentTargetPosition = Vector3.zero;
        public Vector3 MidPosition;
        public bool UseMidPosition = false;
        private List<Transform> targetList = new List<Transform>();
        private int currentTargetIndex = 0;
        GameObject testBall;
        public int PosNeg = -1;

        public float ActivationRange = 0.35f;

        public KeyCode triggerOffKey = KeyCode.Mouse1;
        public KeyCode activation = KeyCode.RightAlt;

        public int Additional360LR = 0;
        public int Additional360UD = 0;
        bool locateMouseOrbit()
        {
            if (theMouseOrbit == null || theMouseOrbit.gameObject == null)
            {
                if (Camera.main != null && Camera.main.GetComponent<MouseOrbit>() != null)
                {
                    theMouseOrbit = Camera.main.GetComponent<MouseOrbit>();
                    mouseOrbitMainCam = Camera.main;
                    if (defaultFoV == 114514)
                    {
                        defaultFoV = mouseOrbitMainCam.fieldOfView;
                    }
                    theMouseOrbit.focusLerpSmooth = 1145141919;
                    //theMouseOrbit.yMaxLimit = 893893;
                    //theMouseOrbit.yMinLimit = -893893;
                    //theMouseOrbit.xSpeed = 99999;
                    //theMouseOrbit.ySpeed = 99999;

                    return true;
                }
                else
                    return false;
            }
            return true;
        }
        void Update()
        {
            //if (testBall == null) { testBall = GameObject.CreatePrimitive(PrimitiveType.Sphere); testBall.transform.position = new Vector3(0, 5, 0); }
            if (StatMaster.isMP) { MPTargetAdd(); } else { SPTargetAdd(); }
            if (!locateMouseOrbit()) { return; }
            if (Enabllled)
            {
                Track();
            }
            else { mouseOrbitMainCam.fieldOfView = Mathf.Lerp(mouseOrbitMainCam.fieldOfView, defaultFoV, 0.025f); }
            if (Input.GetKey(triggerOffKey) || Input.GetKey(KeyCode.F1))
            {
                Enabllled = false;
                theMouseOrbit.xSpeed = 250;
                theMouseOrbit.ySpeed = 120;
                Additional360UD = 0;
                Additional360LR = 0;
            }
            if (Input.GetKeyDown(activation))
            {
                if (Enabllled == true)
                {
                    currentTargetIndex = currentTargetIndex + 1 >= targetList.Count ? 0 : currentTargetIndex + 1;
                }
                CurrentTarget = targetList[currentTargetIndex];
                //CurrentTarget = testBall.transform;
                Enabllled = true;
            };

            //mouseOrbitMainCam.transform.eulerAngles = Vector3.Lerp(
            //    mouseOrbitMainCam.transform.eulerAngles,
            //        new Vector3(
            //            mouseOrbitMainCam.transform.eulerAngles.x,
            //            mouseOrbitMainCam.transform.eulerAngles.y,
            //            mouseOrbitMainCam.transform.eulerAngles.y > 180 || mouseOrbitMainCam.transform.eulerAngles.y < -180 ? 0 : 180
            //            ), 0.2f);
        }

        void MPTargetAdd()
        {
            targetList.Clear();
            if (!GameObject.Find("HUD/MULTIPLAYER/PLAYER_LABELS")) return;
            foreach (PlayerLabel pl in GameObject.Find("HUD/MULTIPLAYER/PLAYER_LABELS").GetComponentsInChildren<PlayerLabel>())
            {
                locateMouseOrbit();
                var thisPos = mouseOrbitMainCam.WorldToScreenPoint(pl.transform.position);
                //bool inSideScreen = mouseOrbitMainCam.pixelRect.Contains(thisPos) && thisPos.z > 0;
                bool inSideScreen = true;
                if (pl.player != null && inSideScreen && pl.player != PlayerData.localPlayer /*&& pl.player.machine.SimulationMachine.FindChild("StartingBlock")*/)
                {
                    //BlockBehaviour nah = null;
                    //while(nah == null || nah.BlockID != 0)
                    //{
                    //    nah = pl.player.machine.gets;
                    //}
                    //targetList.Add(nah.SimBlock.transform);
                    targetList.Add(pl.transform);
                }
            }
        }
        void SPTargetAdd()
        {
            targetList.Clear();
            return;
        }
        void Track()
        {
            CurrentTargetPosition = CurrentTarget.transform.position;

            float pixelWidthF = (float)mouseOrbitMainCam.pixelWidth;
            float pixelHeightF = (float)mouseOrbitMainCam.pixelHeight;

            MidPosition = UseMidPosition? ((CurrentTargetPosition - theMouseOrbit.target.position) / 2) + theMouseOrbit.target.position: CurrentTargetPosition;


            Vector3 targetPositionOnScreen = getCenterXYOfScreen(CurrentTargetPosition);
            if (targetPositionOnScreen.z < 0) { theMouseOrbit.x += 5; theMouseOrbit.y += (CurrentTargetPosition.y <= theMouseOrbit.target.transform.position.y ? -1 : 1); }

            float WidthRatio = Mathf.Abs(targetPositionOnScreen.x) / (pixelWidthF / 2);
            float HeightRatio = Mathf.Abs(targetPositionOnScreen.y) / (pixelHeightF / 2);
            if (targetPositionOnScreen.z < 0
                || WidthRatio > ActivationRange * 2
                || HeightRatio > ActivationRange * 2
                || !CurrentTarget.gameObject.activeSelf
                )
            {
                //BesiegeConsoleController.ShowMessage(string.Format("Out {0}", (0.004f * (CurrentTarget.gameObject.activeSelf ? (WidthRatio + HeightRatio) * (0.35f / ActivationRange) : 5))));
                mouseOrbitMainCam.fieldOfView = Mathf.Lerp(mouseOrbitMainCam.fieldOfView, CurrentTarget.gameObject.activeSelf ? defaultFoV * 2:170, 0.0005f * (WidthRatio + HeightRatio) / (1*ActivationRange));
            }
            else if (targetPositionOnScreen.z < 0
                || inRange(WidthRatio, ActivationRange * 2, ActivationRange)
                || inRange(HeightRatio, ActivationRange * 2, ActivationRange)
                )
            {
                //BesiegeConsoleController.ShowMessage(string.Format("Between {0}", 0.0005f * (WidthRatio + HeightRatio) * (0.35f / ActivationRange)));

                mouseOrbitMainCam.fieldOfView = Mathf.Lerp(mouseOrbitMainCam.fieldOfView, defaultFoV, 0.01f * (WidthRatio + HeightRatio) / (0.5f*ActivationRange));
            }
            else
            {
                //BesiegeConsoleController.ShowMessage(string.Format("InSide {0}", 0.0025f * (2 / (WidthRatio + HeightRatio)) * (0.35f / ActivationRange)));

                mouseOrbitMainCam.fieldOfView = Mathf.Lerp(mouseOrbitMainCam.fieldOfView, defaultFoV, 0.02f * (2 / (WidthRatio + HeightRatio)) / (0.75f*ActivationRange));
            }



            Vector3 MidPositionOnScreen = getCenterXYOfScreen(CurrentTargetPosition);
            if (WidthRatio > ActivationRange)
            {
                this.transform.position = theMouseOrbit.transform.position;
                this.transform.LookAt(MidPosition - this.transform.position);
                float newY = this.transform.eulerAngles.y;
                while (Mathf.Abs(newY - (theMouseOrbit.x - Additional360LR)) > 180)
                {
                    //BesiegeConsoleController.ShowMessage(string.Format("{0} -> {1}, {2}", theMouseOrbit.x, theMouseOrbit.x + Additional360, newY));
                    if (newY > 180) Additional360LR -= 360;
                    else Additional360LR += 360;
                }
                theMouseOrbit.x = Mathf.Lerp(theMouseOrbit.x, newY + Additional360LR, CurrentTarget.gameObject.activeSelf ? 0.05f * WidthRatio / (4*ActivationRange) : 0.8f); 

                //theMouseOrbit.x = Mathf.Lerp(theMouseOrbit.x, theMouseOrbit.x + Mathf.Sign(MidPositionOnScreen.x) * ((float)pixelWidthF) / (1000), 0.25f);
                //theMouseOrbit.y = Mathf.Lerp(theMouseOrbit.y, theMouseOrbit.y + pixelHeightF / 4500 * (CurrentTargetPosition.y <= theMouseOrbit.target.transform.position.y ? -1 : 1), 0.8f);

            }

            if (HeightRatio > ActivationRange)
            {
                this.transform.position = theMouseOrbit.transform.position;
                this.transform.LookAt(MidPosition - this.transform.position);
                float newX = this.transform.eulerAngles.x;
                while (Mathf.Abs(newX - (theMouseOrbit.y - Additional360UD)) > 180)
                {
                    //BesiegeConsoleController.ShowMessage(string.Format("{0} -> {1}, {2}", theMouseOrbit.x, theMouseOrbit.x + Additional360, newY));
                    if (newX > 180) Additional360UD -= 360;
                    else Additional360UD += 360;
                }
                theMouseOrbit.y = Mathf.Lerp(theMouseOrbit.y, newX + Additional360UD, 0.05f * HeightRatio/ (4 * ActivationRange)); 
            }
            //else if (Mathf.Abs(MidPositionOnScreen.y) / (mouseOrbitMainCam.pixelHeight / 2) < 0.4f) theMouseOrbit.y += Math.Sign(MidPositionOnScreen.y) * -1;


            //if (!Input.GetKey(KeyCode.B)) return;
            //Vector3 MeOnTheScreen = getCenterXYOfScreen(theMouseOrbit.target.position);
            //Vector3 RelativeTargetTowardCamera = theMouseOrbit.transform.InverseTransformPoint(CurrentTargetPosition);
            //// Outside or more than 20%
            //if (MeOnTheScreen.z < 0
            //    || Mathf.Abs(MeOnTheScreen.x) / (mouseOrbitMainCam.pixelWidth / 2) > 0.2f
            //    || Mathf.Abs(MeOnTheScreen.y) / (pixelHeightF / 2) > 0.2f
            //    )
            //{
            //    theMouseOrbit.wasdPosOffset = Vector3.Lerp(
            //        theMouseOrbit.wasdPosOffset,
            //        new Vector3(RelativeTargetTowardCamera.x, RelativeTargetTowardCamera.y, theMouseOrbit.distance),
            //        0.2f);
            //}
            ////  Between 10 and 20
            //else if (inRange(MeOnTheScreen.z, theMouseOrbit.distance * 1.2f, theMouseOrbit.distance * 0.7f)
            //    || inRange(Mathf.Abs(MeOnTheScreen.x) / (mouseOrbitMainCam.pixelWidth / 2), 0.2f, 0.1f)
            //    || inRange(Mathf.Abs(MeOnTheScreen.y) / (pixelHeightF / 2), 0.2f, 0.1f)
            //    )
            //{
            //    theMouseOrbit.wasdPosOffset = Vector3.Lerp(theMouseOrbit.wasdPosOffset,
            //        new Vector3(RelativeTargetTowardCamera.x, RelativeTargetTowardCamera.y, -theMouseOrbit.distance),
            //        0.001f);
            //}
            //else if (MeOnTheScreen.z < theMouseOrbit.distance * 0.7f) // Less but too close
            //{
            //    theMouseOrbit.wasdPosOffset = Vector3.Lerp(theMouseOrbit.wasdPosOffset,
            //       -new Vector3(RelativeTargetTowardCamera.x, RelativeTargetTowardCamera.y, theMouseOrbit.distance),
            //        0.2f);
            //}
            //else // Less but too far 
            //{
            //    theMouseOrbit.wasdPosOffset = Vector3.Lerp(theMouseOrbit.wasdPosOffset,
            //        new Vector3(RelativeTargetTowardCamera.x, RelativeTargetTowardCamera.y, theMouseOrbit.distance),
            //        0.2f);
            //}


        }
        Vector3 getCenterXYOfScreen(Vector3 WorldPosition)
        {
            Vector3 middle = new Vector3(mouseOrbitMainCam.pixelWidth / 2, mouseOrbitMainCam.pixelHeight / 2, 0);
            Vector3 thing = mouseOrbitMainCam.WorldToScreenPoint(WorldPosition) - middle;
            return thing;
        }
        bool inRange(float num, float max, float min)
        {
            return num <= max && num >= min;
        }
    }
}

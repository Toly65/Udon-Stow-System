
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class StowManagerExample : UdonSharpBehaviour
{
    
    public StowPoint[] stowPoints;
    public string[] keyForStowPoint; 
    [Header("config")]
    [SerializeField] private float selectionDisplayTime= 0.3f;
    [SerializeField] private float desktopDisplayDistance = 0.3f;
    public Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private int currentActiveStow;
    public AudioSource selectionNoise;
    private VRCPlayerApi localplayer;
    private float avatarSize;
    private float TimerStart;
    private bool display;
    private bool itemStowedInCurrentStow;
    
    private void Start()
    {
        initialPositions = new Vector3[stowPoints.Length];
        initialRotations = new Quaternion[stowPoints.Length];
        for (int i = 0; i < stowPoints.Length; i++)
        {
            initialPositions[i] = stowPoints[i].transform.localPosition;
            initialRotations[i] = stowPoints[i].transform.localRotation;
        }
        localplayer = Networking.LocalPlayer;
    }
    public void PostLateUpdate()
    {
        for (int i = 0; i < keyForStowPoint.Length; i++)
        {
            if (Input.GetKeyDown(keyForStowPoint[i]))
            {
                DisplayStow(i);
            }
        }
        if(display)
        {
            if (Time.time - TimerStart < selectionDisplayTime)
            {
                //keep locked in position
                if(itemStowedInCurrentStow)
                {
                    //check if the stow is in a different state
                    if (!stowPoints[currentActiveStow].GetItemLockState())
                    {
                        ReturnStow();
                        display = false;
                        return;
                    }
                    //item is in stow so to be able to retrieve it it must be in user's face
                    DisplayStowInFront(currentActiveStow);
                }
                else
                {
                    VRC_Pickup pickup = localplayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);
                    if(pickup)
                    {
                        stowPoints[currentActiveStow].ForceItemLock(pickup);
                    }
                }
            }
            else
            {
                //time is up put stow away
                ReturnStow();
            }
        }
        
    }
    public float GetAvatarHeight(VRCPlayerApi player)
    {
        float height = 0;
        Vector3 postition1 = player.GetBonePosition(HumanBodyBones.Head);
        Vector3 postition2 = player.GetBonePosition(HumanBodyBones.Neck);
        height += (postition1 - postition2).magnitude;
        postition1 = postition2;
        postition2 = player.GetBonePosition(HumanBodyBones.Hips);
        height += (postition1 - postition2).magnitude;
        postition1 = postition2;
        postition2 = player.GetBonePosition(HumanBodyBones.RightLowerLeg);
        height += (postition1 - postition2).magnitude;
        postition1 = postition2;
        postition2 = player.GetBonePosition(HumanBodyBones.RightFoot);
        height += (postition1 - postition2).magnitude;
        avatarSize = height;
        for (int i = 0; i < stowPoints.Length; i++)
        {
            stowPoints[i].avatarSize = avatarSize;
        }
        return height;
    }
    private float GetLocalAvatarHeight()
    {
        if (Networking.LocalPlayer == null)
            return 1;
        return GetAvatarHeight(Networking.LocalPlayer);
    }
    private void DisplayStow(int stowID)
    {
        GetLocalAvatarHeight();
        //make sure old stow is in it's original position
        ReturnStow();
        //move the new stow in the correct place
        currentActiveStow = stowID;
        
        if (stowPoints[stowID].GetItemLockState())
        {
            
            itemStowedInCurrentStow = true;
            TimerStart = Time.time;
            display = true;
        }
        else
        {
            
            itemStowedInCurrentStow = false;
            TimerStart = Time.time;
            display = true;
        }
    }
    private void ReturnStow()
    {
        Transform oldStowPointTransform = stowPoints[currentActiveStow].transform;
        oldStowPointTransform.localPosition = initialPositions[currentActiveStow];
        oldStowPointTransform.localRotation = initialRotations[currentActiveStow];
        stowPoints[currentActiveStow].ReturnToDefaultState();
    }

    private void DisplayStowInFront(int stowID)
    {
        //item locked, display in front of player
        Transform stowPointTransform = stowPoints[stowID].transform;
        VRCPlayerApi.TrackingData head = localplayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        Vector3 relativePosition = (head.rotation * Vector3.forward).normalized * desktopDisplayDistance * avatarSize;
        
        stowPointTransform.position = head.position + relativePosition;
        stowPointTransform.rotation = head.rotation;
        stowPointTransform.Rotate(0, 270, 0);
    }

    private void DisplayStowInHand(int stowID)
    {
        //no item locked, put stow in hand
        Transform stowPointTransform = stowPoints[stowID].transform;
        stowPointTransform.position = localplayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
        stowPointTransform.rotation = localplayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
    }

    
}

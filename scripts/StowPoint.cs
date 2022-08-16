
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class StowPoint : UdonSharpBehaviour
{
    [Header("config")]
    [SerializeField] private float proximityDistance;
    [SerializeField] private int sizeClass = 1;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material DefaultMaterial;
    [SerializeField] private Material ActiveMaterial;
    [SerializeField] private HumanBodyBones attachedBone;
    [Header("this may be managed by a stow Manager instead")]
    [SerializeField] private bool onlyRecieveItemsWithStowSettings;
    //[SerializeField] private Material DuplicationMaterial;
    private bool inProximity;
    private bool itemlocked;
    private bool receptive;
    private bool ignoreRightHand;
    private bool ignoreLeftHand;
    private VRC_Pickup recievabePickup;
    private VRCPlayerApi localplayer;
    [HideInInspector] public float avatarSize = 1;
    private void Start()
    {
        localplayer = Networking.LocalPlayer;
    }
    private void PostLateUpdate()
    {
        if (!itemlocked)
        {
            if (!receptive)
            {
                // track hand proximity
                float rightHandDistance = Vector3.Distance(localplayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position, transform.position);
                if (rightHandDistance < proximityDistance && !ignoreRightHand)
                {
                    BecomeReceptive(VRC_Pickup.PickupHand.Right);
                    
                }else if(rightHandDistance > proximityDistance && ignoreRightHand)
                {
                    ignoreRightHand = false;
                }
                float leftHandDistance = Vector3.Distance(localplayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position, transform.position);
                if (leftHandDistance < proximityDistance&& !ignoreLeftHand)
                {
                    BecomeReceptive(VRC_Pickup.PickupHand.Left);
                }else if(leftHandDistance > proximityDistance && ignoreLeftHand)
                {
                    ignoreLeftHand = false;
                }

            }
            else
            {
                //track how far the pickup goes
                float distance = Vector3.Distance(recievabePickup.transform.position, transform.position);
                if(distance > proximityDistance)
                {
                    ReturnToDefaultState();
                    return;
                }
                if(recievabePickup)
                {
                    if (!recievabePickup.IsHeld)
                    {
                        lockItem(recievabePickup);
                    }
                }
                
            }
        }
        else
        {
            //check if the item is being held, if not lock its position
            if(recievabePickup.IsHeld)
            {
                targetRenderer.material = ActiveMaterial;
                
                itemlocked = false;
            }
            recievabePickup.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
    }

    private void BecomeReceptive(VRC_Pickup.PickupHand hand)
    {
        //pickup identification  
        recievabePickup = localplayer.GetPickupInHand(hand);
        if(recievabePickup)
        {
            StowSettings settings = recievabePickup.transform.GetChild(0).GetComponent<StowSettings>();
            if (onlyRecieveItemsWithStowSettings&&!settings)
            {
                if(hand == VRC_Pickup.PickupHand.Right)
                {
                    ignoreRightHand = true;
                }
                else
                {
                    ignoreLeftHand = true;
                }
                return;
            }
            if(settings)
            {
                //check the size
                if(settings.GetSizeClass() > sizeClass)
                {
                    //size too big, ignore
                    if (hand == VRC_Pickup.PickupHand.Right)
                    {
                        ignoreRightHand = true;
                    }
                    else
                    {
                        ignoreLeftHand = true;
                    }
                    return;
                }
            }
            //material swap
            targetRenderer.material = ActiveMaterial;
            //haptics    
            localplayer.PlayHapticEventInHand(hand, 0.1f, 1, 1);
            receptive = true;
        }
        
    }
    public void ReturnToDefaultState()
    {
        targetRenderer.material = DefaultMaterial;
        receptive = false;
    }

    public void ForceItemLock(VRC_Pickup pickup)
    {
        StowSettings settings = pickup.transform.GetChild(0).GetComponent<StowSettings>();
        if (onlyRecieveItemsWithStowSettings && !settings)
        {
            return;
        }
        if(settings)
        {
            
            //check the size
            if (settings.GetSizeClass() > sizeClass)
            {
                //size too big, ignore
                return;
            }
            
        }
        pickup.Drop();
        recievabePickup = pickup;
        lockItem(pickup);
    }
    public void ForceReleaseItemLock()
    {
        itemlocked = false;
    }
    private void lockItem(VRC_Pickup pickup)
    {
        
        targetRenderer.material = DefaultMaterial;
        itemlocked = true;
    }
    public int GetSizeClass()
    {
        return sizeClass;
    }
    public bool GetItemLockState()
    {
        return itemlocked;
    }
}

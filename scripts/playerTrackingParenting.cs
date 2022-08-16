
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class playerTrackingParenting : UdonSharpBehaviour
{
    public Transform mainTransform;
    public Transform hipTransform;
    public Transform TorsoTransform;
    private float avatarSize;
    private VRCPlayerApi localplayer;

    private void Start()
    {
        localplayer = VRCPlayerApi.GetPlayerById(Networking.LocalPlayer.playerId);
    }
    public void InputGrab(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        float scale = GetLocalAvatarHeight();
        mainTransform.localScale = new Vector3(scale, scale, scale);
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
        return height;
    }
    private float GetLocalAvatarHeight()
    {
        if (Networking.LocalPlayer == null)
            return 1;
        return GetAvatarHeight(Networking.LocalPlayer);
    }

    public void Update()
    {
        if(hipTransform)
        {
            hipTransform.SetPositionAndRotation(localplayer.GetBonePosition(HumanBodyBones.Hips), localplayer.GetBoneRotation(HumanBodyBones.Hips));
            
        }
        if (TorsoTransform)
        {
            TorsoTransform.SetPositionAndRotation(localplayer.GetBonePosition(HumanBodyBones.Chest), localplayer.GetBoneRotation(HumanBodyBones.Chest));
        }
    }
}

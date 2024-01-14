
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
    public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float prevEyeHeightAsMeters)
    {
        mainTransform.localScale = new Vector3(player.GetAvatarEyeHeightAsMeters(), player.GetAvatarEyeHeightAsMeters(), player.GetAvatarEyeHeightAsMeters());
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

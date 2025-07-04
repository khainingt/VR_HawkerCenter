/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using UnityEngine;

namespace OculusSampleFramework
{
    public class PanelHMDFollower : MonoBehaviour
    {
        [SerializeField] private float followDistance = 2.0f; // Distance from the user's position
        [SerializeField] private float followSpeed = 5.0f; // Speed at which the panel follows
        [SerializeField] private float handHeightOffset = 0.1f; // Offset to place panel near the player's hand
        [SerializeField] private float activationDistance = 4.0f; // Distance threshold to trigger movement

        private OVRCameraRig _cameraRig;

        private void Awake()
        {
            _cameraRig = FindObjectOfType<OVRCameraRig>();
            if (_cameraRig == null)
            {
                Debug.LogError("OVRCameraRig not found in the scene.");
            }
        }

        private void Update()
        {
            if (_cameraRig == null) return;

            Vector3 playerPosition = _cameraRig.centerEyeAnchor.position;
            Vector3 panelPosition = transform.position;

            // Check if the panel is far from the player
            if (Vector3.Distance(playerPosition, panelPosition) > activationDistance)
            {
                // Move panel to follow player's position
                Vector3 targetPosition = playerPosition + _cameraRig.centerEyeAnchor.forward * followDistance;

                // Adjust height based on player's right hand position
                Vector3 rightHandPosition = _cameraRig.rightHandAnchor.position;
                targetPosition.y = rightHandPosition.y + handHeightOffset;

                // Smoothly move the panel to the target position
                transform.position = Vector3.Lerp(panelPosition, targetPosition, Time.deltaTime * followSpeed);

                // Rotate the panel to face the player
                Vector3 lookDirection = playerPosition - transform.position;
                lookDirection.y = 0; // Keep the panel upright
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
}

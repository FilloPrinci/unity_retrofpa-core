using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Toggleable screen showing a fixed grid of slots (empty ones included)
    /// reflecting <see cref="InventoryManager"/>. Clicking a slot selects it,
    /// showing its item's name/description and, for equippable items, an
    /// Equip/Unequip button. The currently equipped item's
    /// <see cref="ItemData.WorldPrefab"/> is rendered live via
    /// <see cref="previewCamera"/> into <see cref="previewImage"/> (a
    /// RenderTexture created at runtime — see <see cref="SetUpPreview"/>).
    /// </summary>
    public class InventoryUIController : UIScreen
    {
        [Header("Grid")]
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField] private RectTransform slotsContainer;
        [SerializeField] private InventorySlotUI slotPrefab;

        [Header("Detail Panel")]
        [SerializeField] private TMP_Text detailNameText;
        [SerializeField] private TMP_Text detailDescriptionText;
        [SerializeField] private Button equipButton;
        [SerializeField] private TMP_Text equipButtonLabelText;
        [SerializeField] private Button closeButton;

        [Header("3D Preview")]
        [Tooltip("The player (or whichever GameObject should be passed as the 'wielder' to Equip/Unequip).")]
        [SerializeField]
        private GameObject wielder;
        [SerializeField] private RawImage previewImage;
        [SerializeField] private Camera previewCamera;
        [SerializeField] private Transform previewAnchor;
        [SerializeField] private float previewSpinSpeed = 30f;

        [Header("Localization")]
        [SerializeField] private LocalizedString equipLabel;
        [SerializeField] private LocalizedString unequipLabel;

        private readonly List<InventorySlotUI> spawnedSlots = new();
        private RenderTexture previewRenderTexture;
        private GameObject previewInstance;
        private int selectedSlotIndex = -1;

        protected override void Awake()
        {
            base.Awake();
            closeButton?.onClick.AddListener(Hide);
            equipButton?.onClick.AddListener(HandleEquipClicked);
            SetUpPreview();
        }

        private void SetUpPreview()
        {
            if (previewCamera == null || previewImage == null)
            {
                return;
            }

            previewRenderTexture = new RenderTexture(256, 256, 16);
            previewCamera.targetTexture = previewRenderTexture;
            previewImage.texture = previewRenderTexture;
        }

        private void OnEnable()
        {
            InventoryManager.SlotChanged += HandleSlotChanged;
            InventoryManager.EquippedItemChanged += HandleEquippedItemChanged;

            if (toggleAction != null)
            {
                toggleAction.action.Enable();
                toggleAction.action.performed += HandleTogglePerformed;
            }
        }

        private void OnDisable()
        {
            InventoryManager.SlotChanged -= HandleSlotChanged;
            InventoryManager.EquippedItemChanged -= HandleEquippedItemChanged;

            if (toggleAction != null)
            {
                toggleAction.action.performed -= HandleTogglePerformed;
                toggleAction.action.Disable();
            }
        }

        private void Update()
        {
            if (previewInstance != null && previewSpinSpeed != 0f)
            {
                previewInstance.transform.Rotate(Vector3.up, previewSpinSpeed * Time.deltaTime, Space.World);
            }
        }

        private void HandleTogglePerformed(InputAction.CallbackContext context)
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
                RebuildGrid();
                RefreshPreview();
            }
        }

        private void RebuildGrid()
        {
            foreach (InventorySlotUI slot in spawnedSlots)
            {
                Destroy(slot.gameObject);
            }
            spawnedSlots.Clear();
            selectedSlotIndex = -1;

            if (InventoryManager.Instance == null || slotsContainer == null || slotPrefab == null)
            {
                ClearDetail();
                return;
            }

            for (int i = 0; i < InventoryManager.Instance.Capacity; i++)
            {
                InventorySlotUI slot = Instantiate(slotPrefab, slotsContainer);
                slot.gameObject.SetActive(true);
                slot.SetEmpty();

                int index = i;
                slot.Clicked += () => SelectSlot(index);

                spawnedSlots.Add(slot);
            }

            IReadOnlyList<InventoryEntry> currentSlots = InventoryManager.Instance.Slots;
            for (int i = 0; i < currentSlots.Count && i < spawnedSlots.Count; i++)
            {
                if (currentSlots[i] != null)
                {
                    spawnedSlots[i].Set(currentSlots[i].Item, currentSlots[i].Quantity);
                }
            }

            ClearDetail();
        }

        private void HandleSlotChanged(int index, InventoryEntry entry)
        {
            if (index < 0 || index >= spawnedSlots.Count)
            {
                return;
            }

            if (entry == null)
            {
                spawnedSlots[index].SetEmpty();
            }
            else
            {
                spawnedSlots[index].Set(entry.Item, entry.Quantity);
            }

            if (index == selectedSlotIndex)
            {
                UpdateDetailForSelection();
            }
        }

        private void SelectSlot(int index)
        {
            selectedSlotIndex = index;
            for (int i = 0; i < spawnedSlots.Count; i++)
            {
                spawnedSlots[i].SetSelected(i == index);
            }

            UpdateDetailForSelection();
        }

        private InventoryEntry GetSelectedEntry()
        {
            if (InventoryManager.Instance == null || selectedSlotIndex < 0)
            {
                return null;
            }

            IReadOnlyList<InventoryEntry> currentSlots = InventoryManager.Instance.Slots;
            return selectedSlotIndex < currentSlots.Count ? currentSlots[selectedSlotIndex] : null;
        }

        private void UpdateDetailForSelection()
        {
            InventoryEntry entry = GetSelectedEntry();
            if (entry == null)
            {
                ClearDetail();
                return;
            }

            if (detailNameText != null)
            {
                detailNameText.text = entry.Item.DisplayName.GetLocalizedString();
            }

            if (detailDescriptionText != null)
            {
                detailDescriptionText.text = entry.Item.Description.GetLocalizedString();
            }

            equipButton?.gameObject.SetActive(entry.Item.IsEquippable);
            UpdateEquipButtonLabel();
        }

        private void ClearDetail()
        {
            if (detailNameText != null)
            {
                detailNameText.text = string.Empty;
            }

            if (detailDescriptionText != null)
            {
                detailDescriptionText.text = string.Empty;
            }

            equipButton?.gameObject.SetActive(false);
        }

        private void UpdateEquipButtonLabel()
        {
            if (equipButtonLabelText == null)
            {
                return;
            }

            InventoryEntry entry = GetSelectedEntry();
            bool isEquipped = entry != null && InventoryManager.Instance != null && InventoryManager.Instance.EquippedItem == entry.Item;
            LocalizedString label = isEquipped ? unequipLabel : equipLabel;
            if (!label.IsEmpty)
            {
                equipButtonLabelText.text = label.GetLocalizedString();
            }
        }

        private void HandleEquipClicked()
        {
            InventoryEntry entry = GetSelectedEntry();
            if (entry == null || !entry.Item.IsEquippable || InventoryManager.Instance == null)
            {
                return;
            }

            if (InventoryManager.Instance.EquippedItem == entry.Item)
            {
                InventoryManager.Instance.Unequip(wielder);
            }
            else
            {
                InventoryManager.Instance.Equip(entry.Item, wielder);
            }
        }

        private void HandleEquippedItemChanged(ItemData previous, ItemData current)
        {
            RefreshPreview();
            UpdateEquipButtonLabel();
        }

        private void RefreshPreview()
        {
            if (previewInstance != null)
            {
                Destroy(previewInstance);
                previewInstance = null;
            }

            ItemData equipped = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedItem : null;
            bool hasPreview = equipped != null && equipped.WorldPrefab != null && previewAnchor != null;

            if (hasPreview)
            {
                previewInstance = Instantiate(equipped.WorldPrefab, previewAnchor);
                previewInstance.transform.localPosition = Vector3.zero;
                previewInstance.transform.localRotation = Quaternion.identity;
                SetLayerRecursively(previewInstance, previewAnchor.gameObject.layer);
            }

            if (previewImage != null)
            {
                previewImage.enabled = hasPreview;
            }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}

using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.Input;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.UI.Fabricator;
using Il2CppMonomiPark.SlimeRancher.UI.Framework.CommonControls;
using Il2CppMonomiPark.SlimeRancher.UI.Pedia;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace Variants.Harmony
{
    [HarmonyPatch(typeof(InputDirector), nameof(InputDirector.Awake))]
    internal static class InputDirectorAwakePatch
    {
        private static PediaRuntimeCategory PreviousCategory;
        private static PediaEntry PreviousEntry;

        ///

        internal static InputEvent InputEvent;

        ///

        public static FabricatorSoundBank SoundBank { get; set; }

        public static PediaCategory Category => LookupDirectorAwakePatch.VariantsCategory;
        public static PediaRuntimeCategory RuntimeCategory => LookupDirectorAwakePatch.VariantsRuntimeCategory;

        public static PediaEntry Locked => LookupDirectorAwakePatch.LockedEntry;
        public static PediaEntry LockedSlime => LookupDirectorAwakePatch.LockedSlimeEntry;

        ///

        public static void Prefix(InputDirector __instance)
        {
            Il2CppSystem.Action<InputEventData> onViewVariants = new Action<InputEventData>(OnViewVariants);

            ///

            InputActionMap paused = __instance._inputActions.FindActionMap("Paused");
            paused.Disable();

            InputAction inputAction = __instance._inputActions.FindActionMap("Paused").AddAction("View Variants", InputActionType.Button, "<Keyboard>/z", groups: "PC_KeyboardMouse");
            inputAction.AddBinding("<Gamepad>/buttonNorth", groups: "PC_Gamepad_Xbox;PC_Gamepad_DualShock;PC_Joystick");

            paused.Enable();

            ///

            InputEvent = ScriptableObject.CreateInstance<InputEvent>();
            InputEvent.hideFlags |= HideFlags.HideAndDontSave;
            InputEvent.name = "View Variants";

            InputEvent.Performed += onViewVariants;

            ///

            InputActionReference inputActionReference = ScriptableObject.CreateInstance<InputActionReference>();
            inputActionReference.hideFlags |= HideFlags.HideAndDontSave;
            inputActionReference.Set(inputAction);

            InputEventBinding inputEventBinding = new()
            {
                _input = inputActionReference,
                _inputEvents = new InputEvent[1] { InputEvent }
            };

            __instance._inputActionController._inputBindings = __instance._inputActionController._inputBindings.ToArray().AddToArray(inputEventBinding);
        }

        ///

        static void SetPrevious(PediaRuntimeCategory rc, PediaEntry e)
        {
            PreviousCategory = rc;
            PreviousEntry = e;
        }

        static void TransitionCurrentScreen(PediaRoot r, PediaScreen s, Action a)
        {
            r.StartCoroutine(r.FadeOut(s.CanvasGroup));

            a.Invoke();

            r.StartCoroutine(r.FadeIn(s.CanvasGroup));
        }

        static void OnViewVariants(InputEventData data)
        {
            PediaRoot pediaRoot = Get<GameObject>("PediaUI(Clone)")?.GetComponent<PediaRoot>();
            PediaSlimeAndResourceScreen pediaScreen = pediaRoot?._currentScreen?.TryCast<PediaSlimeAndResourceScreen>();

            ///

            if (pediaScreen)
            {
                if (pediaScreen.Category.Equals(RuntimeCategory) && PreviousCategory.IsNotNull())
                {
                    TransitionCurrentScreen(pediaRoot, pediaScreen, () =>
                    {
                        pediaScreen.SetCategory(PreviousCategory);

                        if (PreviousEntry.IsNotNull())
                            pediaScreen.ViewEntry(PreviousEntry);

                        SetPrevious(null, null);
                    });

                    pediaScreen._soundBank.PlayCloseCategory();
                    return;
                }
                else
                    SetPrevious(null, null);

                ///

                IdentifiablePediaEntry entry = pediaScreen._details._currentEntry.TryCast<IdentifiablePediaEntry>();
                if (entry.IsNull() || !VariantPedias.ContainsKey(entry.IdentifiableType?.ReferenceId))
                    return;

                ///

                if (entry.IdentifiableType.TryCast<SlimeDefinition>())
                    Category._lockedEntry = LockedSlime;
                else
                    Category._lockedEntry = Locked;

                Category._items = VariantPedias[entry.IdentifiableType.ReferenceId];
                RuntimeCategory._items = new();

                foreach (var x in VariantPedias[entry.IdentifiableType.ReferenceId])
                    RuntimeCategory.AddDynamicItem(x);

                ///

                if (!pediaScreen.Category.Equals(RuntimeCategory))
                {
                    TransitionCurrentScreen(pediaRoot, pediaScreen, () =>
                    {
                        SetPrevious(pediaScreen._category, entry);
                        pediaScreen.SetCategory(RuntimeCategory);
                    });

                    if (SoundBank)
                        SoundBank.PlayOpened();
                    else
                    {
                        SoundBank = Get<FabricatorSoundBank>("FabricatorSoundBank");
                        SoundBank.PlayOpened();
                    }
                }
            }
        }
    }
}

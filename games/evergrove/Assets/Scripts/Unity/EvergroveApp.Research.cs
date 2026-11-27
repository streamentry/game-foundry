using System;
using System.Collections;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Research-only controls compile only in the Editor or explicitly
        // opted-in Development Builds, never in ordinary player builds.
        // Hidden in the M3 participant view so the study does not reveal its hypothesis.
        private void BuildResearchControls(bool living)
        {
            // A mode comparison requires matched puzzle fixtures before any
            // research result can be used as evidence of narrative value.
            if (world.CanSwitchMode)
            {
                Label(content, "Comparison hint",
                    GameLocalization.Text("playtest"), 11, 21, Moss);
                var modes = Row(content, "Comparison modes");
                Button(modes.transform, "Living mode", GameLocalization.Text("living"), () => SwitchMode(PlayMode.Living),
                    52, living);
                Button(modes.transform, "Puzzle control", GameLocalization.Text("puzzle"), () => SwitchMode(PlayMode.PuzzleOnly),
                    52, !living);
            }

            // Matched M4 control: facilitator-only. Its strings are English
            // literals by design (the 81-key player catalog would need 30
            // unreviewed translations for dev-only UI). Nothing here compiles
            // into a player build, and the entry is hidden while an unreadable
            // save blocks writes so an unreadable save cannot be silently replaced.
            if (living && world.Choices.Length >= WorldModel.ScheduleLength)
            {
                Button(content, "Copy matched schedule", "Copy this Living run's schedule", CopyLivingSchedule, 48);
                if (matchedScheduleCopied)
                    Label(content, "Schedule copied", "Copied privately", 12, 43, Moss, TextAnchor.MiddleCenter);
            }
            if (!WorldStore.RecoveryBlocked)
            {
                Button(content, "Matched control entry",
                    matchedEntryOpen ? "Close matched control" : "Start matched control (dev)",
                    () => {
                        matchedEntryOpen = !matchedEntryOpen;
                        confirmMatchedReplace = false;
                        pendingMatchedSchedule = null;
                        matchedError = "";
                        Rebuild();
                    }, 48);
                if (matchedEntryOpen) BuildMatchedEntry();
            }

            // Puzzle-only condition must not display or switch into the
            // changing village art: doing so would confound the experiment.
            if (living)
            {
                // Two comparable art renderers using the same saved decisions.
                // Keep this manual A/B surface below the gameplay.
                Button(content, "Diorama visual comparison",
                    GameLocalization.Text(showMiniature ? "art_3d" : "art_2d"), () => {
                        showMiniature = !showMiniature;
                        if (localPlaytest != null) localPlaytest.ChangeVisual(showMiniature, Time.unscaledTime);
                        PlayerPrefs.SetInt("evergrove:visual-miniature", showMiniature ? 1 : 0);
                        PlayerPrefs.Save();
                        Rebuild();
                    }, 49);
                if (showMiniature)
                    Button(content, "Optional miniature motion",
                        GameLocalization.Text(animateMiniature ? "motion_on" : "motion_off"), () => {
                            animateMiniature = !animateMiniature;
                            PlayerPrefs.SetInt(ReducedMotionKey, animateMiniature ? 1 : 0);
                            PlayerPrefs.Save();
                            Rebuild();
                        }, 48);
            }
        }

        // Local playtest report and return diary (facilitator-only, opt-in).
        private void BuildLocalStudyControls()
        {
            if (localPlaytest == null)
            {
                Button(content, "Playtest consent", GameLocalization.Text("record"),
                    StartParticipantView, 48);
                Label(content, "No tracking by default",
                    GameLocalization.Text("privacy"),
                    12, 43, Moss, TextAnchor.MiddleCenter);
            }
            else
            {
                Button(content, "Copy playtest", GameLocalization.Text("copy"),
                    () => {
                        GUIUtility.systemCopyBuffer = localPlaytest.Report(Time.unscaledTime);
                        playtestCopied = true;
                        Rebuild();
                    }, 48);
                if (playtestCopied)
                    Label(content, "Copied privately",
                        GameLocalization.Text("copied"),
                        12, 43, Moss, TextAnchor.MiddleCenter);
                Button(content, "Stop playtest", GameLocalization.Text("clear"),
                    () => { localPlaytest = null; playtestCopied = false; Rebuild(); }, 48);
            }
            // Return diary: explicit two-step consent; dates only; copy or delete in one action.
            Label(content, "Return diary", returnDiary.Consented
                ? returnDiary.Days.Count + " local dates recorded on this device"
                : "not recording", 12, 43, Moss, TextAnchor.MiddleCenter);
            if (!returnDiary.Consented && !returnDiaryAsked)
            {
                Button(content, "Return diary consent", "Start return diary",
                    () => { returnDiaryAsked = true; Rebuild(); }, 48);
            }
            else if (!returnDiary.Consented)
            {
                Label(content, "Consent",
                    "Records only the local dates this app is opened on this device. Nothing is uploaded. Delete any time.",
                    12, 43, Moss, TextAnchor.MiddleCenter);
                Button(content, "Agree and record dates", "Agree and record dates",
                    () => {
                        returnDiary.GrantConsent(DateTime.Now.Date);
                        SaveReturnDiary();
                        returnDiaryAsked = false;
                        Rebuild();
                    }, 48);
                Button(content, "Not now", "Not now",
                    () => { returnDiaryAsked = false; Rebuild(); }, 48);
            }
            else
            {
                Button(content, "Copy return diary", "Copy return diary",
                    () => {
                        GUIUtility.systemCopyBuffer = returnDiary.Export(DateTime.Now.Date);
                        returnDiaryCopied = true;
                        Rebuild();
                    }, 48);
                if (returnDiaryCopied)
                    Label(content, "Copied privately", "Copied privately",
                        12, 43, Moss, TextAnchor.MiddleCenter);
                Button(content, "Delete return diary", "Delete return diary",
                    () => { ClearReturnDiary(); Rebuild(); }, 48);
            }
        }

        // M3 participant view: starting the local report hides every research
        // and developer control and returns to the top of the page. Holding the
        // wordmark for FacilitatorHold.HoldSeconds brings the controls back.
        private void StartParticipantView()
        {
            localPlaytest = new LocalPlaytest(world.Mode, showMiniature, world.Round, Time.unscaledTime);
            playtestCopied = false;
            participantView = true;
            Rebuild(true);
        }

        private void EndParticipantView()
        {
            participantView = false;
            Rebuild();
        }
#endif

        // One fruit to a neighbour (I231): they remember it truthfully (no meter).
        private void GiveFruit(int villager)
        {
            if (!world.GiveFruit(villager)) return;
            SaveWorld();
            PlayChime();
            OpenConversation(villager, true); // their next line is the thank-you
        }
        private int fruitThanks = -1; // the neighbour just given fruit: thanks come first (I231)

        private void OpenConversation(int villagerIndex, bool afterFruit = false)
        {
            fruitThanks = afterFruit ? villagerIndex : -1;
            // The "together" line is said once: talking to anyone else lets it pass (I266).
            if (togetherKeeper >= 0 && villagerIndex != togetherKeeper) togetherKeeper = -1;
            speakingWith = villagerIndex;
            if (world.Mode == PlayMode.Living)
            {
                Fact said = CompanionPick(villagerIndex);
                string line = NeighborSays(villagerIndex);
                PlayerPrefs.SetString(HeardKey + villagerIndex, line);
                // SC02: remembered only when the companion's remark is what was shown (not a "speak less"
                // silence, v1 #85, nor an arc line, v1 #83); after the line is taken, so rebuilds keep it.
                if (said != null && line == CompanionWords(said, villagerIndex, out _)) CompanionHeard(villagerIndex, said);
                PlayerPrefs.Save();
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Talk(Time.unscaledTime);
#endif
            Rebuild(); // the card floats over the page; the scroll stays put (I127)
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void SwitchMode(PlayMode mode)
        {
            if (!world.CanSwitchMode) return;
            speakingWith = -1;
            world = WorldModel.New(mode);
            if (localPlaytest != null) localPlaytest.ChangeMode(mode, Time.unscaledTime);
            SaveWorld();
            Rebuild();
        }

        private void CopyLivingSchedule()
        {
            if (world.Mode != PlayMode.Living || world.Choices.Length < WorldModel.ScheduleLength) return;
            GUIUtility.systemCopyBuffer = WorldModel.FormatSchedule(
                WorldModel.ScheduleFromChoices(world.Choices));
            matchedScheduleCopied = true;
            Rebuild();
        }

        private void BuildMatchedEntry()
        {
            var card = Card(content, "Matched control card", Sand);
            Label(card.transform, "Matched explanation",
                "Enter 11 digits, each 1, 2 or 3: the variant of boards 2 to 12. " +
                "This run shows no village choices or story.", 12, 52, Ink);
            CreateMatchedInput(card.transform);
            if (matchedError.Length > 0)
                Label(card.transform, "Schedule error", matchedError, 13, 36, MosseloraTheme.EarthInk);
            if (!confirmMatchedReplace)
                Button(card.transform, "Check matched schedule", "Check schedule", RequestMatchedStart, 50, true);
            else
            {
                Label(card.transform, "Replace warning",
                    "This replaces the saved garden. Tap again to confirm.", 13, 40, Ink);
                Button(card.transform, "Confirm matched replace", GameLocalization.Text("erase"),
                    ConfirmMatchedStart, 52, true);
                Button(card.transform, "Cancel matched replace", GameLocalization.Text("keep"), () => {
                    confirmMatchedReplace = false;
                    pendingMatchedSchedule = null;
                    Rebuild();
                }, 48);
            }
        }

        private void CreateMatchedInput(Transform parent)
        {
            var node = Node("Matched schedule input", parent);
            MinHeight(node, TextScale.Scale(48));
            var background = node.AddComponent<Image>();
            background.color = White;
            var field = node.AddComponent<InputField>();

            var textNode = Node("Schedule text", node.transform);
            Fill(textNode.GetComponent<RectTransform>());
            textNode.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 0f);
            textNode.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, 0f);
            var text = textNode.AddComponent<Text>();
            StyleText(text, 16);
            text.color = Ink;
            text.supportRichText = false;
            text.raycastTarget = false;

            var placeholderNode = Node("Schedule placeholder", node.transform);
            Fill(placeholderNode.GetComponent<RectTransform>());
            placeholderNode.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 0f);
            placeholderNode.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, 0f);
            var placeholder = placeholderNode.AddComponent<Text>();
            StyleText(placeholder, 16);
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = Moss;
            placeholder.supportRichText = false;
            placeholder.raycastTarget = false;
            placeholder.text = "e.g. 12332112312";

            field.targetGraphic = background;
            field.textComponent = text;
            field.placeholder = placeholder;
            field.contentType = InputField.ContentType.Standard;
            field.lineType = InputField.LineType.SingleLine;
            field.characterLimit = 0;
            field.text = matchedDraft;
            field.onValueChanged.AddListener(value => matchedDraft = value ?? "");
        }

        private void RequestMatchedStart()
        {
            int[] schedule;
            if (!WorldModel.TryParseSchedule((matchedDraft ?? "").Trim(), out schedule))
            {
                matchedError = "Enter exactly 11 digits, each 1, 2 or 3.";
                Rebuild();
                return;
            }
            matchedError = "";
            pendingMatchedSchedule = schedule;
            confirmMatchedReplace = true;
            Rebuild();
        }

        private void ConfirmMatchedStart()
        {
            if (pendingMatchedSchedule == null || WorldStore.RecoveryBlocked)
            {
                confirmMatchedReplace = false;
                pendingMatchedSchedule = null;
                Rebuild();
                return;
            }
            world = WorldModel.NewMatched(pendingMatchedSchedule);
            if (localPlaytest != null) localPlaytest.ChangeMode(PlayMode.PuzzleMatched, Time.unscaledTime);
            pendingMatchedSchedule = null;
            confirmMatchedReplace = false;
            matchedEntryOpen = false;
            matchedDraft = "";
            matchedError = "";
            matchedScheduleCopied = false;
            speakingWith = -1;
            showingMemory = false;
            showJournal = false;
            SaveWorld();
            Rebuild(true);
        }

#endif
    }
}

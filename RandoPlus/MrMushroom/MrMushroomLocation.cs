using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RandoPlus.MrMushroom
{
    public class MrMushroomLocation : ContainerLocation
    {
        public int mushroomState;
        public string objectName;

        public bool MushroomUnlocked() => PlayerData.instance.GetInt(nameof(PlayerData.mrMushroomState)) >= mushroomState;
        public bool SuccessfullyInteracted() => Placement.CheckVisitedAll(VisitState.Accepted);
        public bool Appears() => MushroomUnlocked()
            && !SuccessfullyInteracted()
            && !Placement.AllObtained();

        protected override void OnLoad()
        {
            Events.AddFsmEdit(sceneName, new(objectName, "Control"), ModifyMrMushroom);
            Events.AddFsmEdit(sceneName, new(objectName, "Conversation Control"), ModifyMushConvo);
        }
        protected override void OnUnload()
        {
            Events.RemoveFsmEdit(sceneName, new(objectName, "Control"), ModifyMrMushroom);
            Events.RemoveFsmEdit(sceneName, new(objectName, "Conversation Control"), ModifyMushConvo);
        }

        private void ModifyMushConvo(PlayMakerFSM fsm)
        {
            FsmState convo = fsm.GetState("Convo");
            convo.RemoveActionsOfType<IncrementPlayerDataInt>();
            convo.Actions[0] = new Lambda(() => fsm.FsmVariables.GetFsmInt("Mushroom State").Value = mushroomState);

            fsm.GetState("Send Rocket").AddFirstAction(new Lambda(() => Placement.AddVisitFlag(VisitState.Accepted)));
            fsm.GetState("Send Leave").AddFirstAction(new Lambda(() => Placement.AddVisitFlag(VisitState.Accepted)));
        }

        private void ModifyMrMushroom(PlayMakerFSM fsm)
        {
            fsm.GetState("Check").Actions[1] = new DelegateBoolTest(Appears, "FINISHED", "DESTROY");

            fsm.GetState("Left").AddFirstAction(new Lambda(() => PlaceContainer(fsm.gameObject)));
            fsm.GetState("Box Down").AddFirstAction(new Lambda(() => PlaceContainer(fsm.gameObject)));
            fsm.GetState("Destroy").AddFirstAction(new Lambda(() => 
            { 
                // This handles respawned items and geo rock shells, as well as dropped but unclaimed items
                if (SuccessfullyInteracted())
                {
                    PlaceContainer(fsm.gameObject);
                }
            }));
        }

        private void PlaceContainer(GameObject mush)
        {
            base.GetContainer(out GameObject obj, out string containerType);
            Container.GetContainer(containerType).ApplyTargetContext(obj, mush, 0);
            if (containerType == Container.Shiny && !Placement.GetPlacementAndLocationTags().OfType<ShinyFlingTag>().Any())
            {
                ShinyUtility.SetShinyFling(obj.LocateMyFSM("Shiny Control"), ShinyFling.RandomLR);
            }
        }
    }
}

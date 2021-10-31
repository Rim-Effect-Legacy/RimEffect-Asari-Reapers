using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimEffectAsari
{
    using System.Runtime.Remoting.Messaging;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using Verse.Sound;

    public class JobDriver_BansheeTeleport : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil waitToil = Toils_General.Wait(GenTicks.TicksPerRealSecond, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A);
            waitToil.AddPreInitAction(() => REA_DefOf.RE_Pawn_Husk_Banshee_Call.PlayOneShot(SoundInfo.InMap(this.pawn)));
            yield return waitToil;
            yield return new Toil()
                         {
                             initAction = () =>
                                          {
                                              Vector3 spawnPosition = this.pawn.Position.ToVector3();
                                              FleckMaker.ThrowAirPuffUp(spawnPosition, this.pawn.Map);
                                              FleckMaker.ThrowLightningGlow(spawnPosition, this.Map, 5f);
                                              FleckMaker.ThrowAirPuffUp(this.TargetA.CenterVector3, this.pawn.Map);
                                              FleckMaker.ThrowLightningGlow(this.TargetA.CenterVector3, this.Map, 5f);
                                              this.pawn.Position = this.TargetA.Thing.Position;
                                          }
                         };
        }
    }
}

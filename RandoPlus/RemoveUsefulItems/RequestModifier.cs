using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace RandoPlus.RemoveUsefulItems
{
    public static class RequestModifier
    {
        public record struct RemoverInfo
        (
            string OldItem,
            string VanillaLocation,
            string SkipSetting,
            string NewItem,
            Func<bool> IsActive
        );

        public static readonly RemoverInfo[] settings = new RemoverInfo[]
        {
            new(ItemNames.Lumafly_Lantern, LocationNames.Sly, nameof(SkipSettings.DarkRooms), Consts.NoLantern,() => RandoPlus.GS.没有灯笼 ),
            new(ItemNames.Ismas_Tear, LocationNames.Ismas_Tear, nameof(SkipSettings.AcidSkips), Consts.NoTear, () => RandoPlus.GS.没有酸泪 ),
            new(ItemNames.Swim, null, nameof(SkipSettings.AcidSkips), Consts.NoSwim, () => RandoPlus.GS.没有游泳 )
        };

        public static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(-499f, SetupRefs);

            foreach (RemoverInfo info in settings)
            {
                RequestBuilder.OnUpdate.Subscribe(50f, CreateRemover(info));
            }
        }

        private static void SetupRefs(RequestBuilder rb)
        {
            if (!RandoPlus.GS.Any) return;

            foreach (RemoverInfo ri in settings)
            {
                rb.EditItemRequest(ri.NewItem, info =>
                {
                    info.getItemDef = () => new ItemDef()
                    {
                        Name = ri.NewItem,
                        Pool = Consts.RemoveUsefulItems,
                        MajorItem = false,
                        PriceCap = 500
                    };
                });

                rb.OnGetGroupFor.Subscribe(-999f, MatchGroup);

                bool MatchGroup(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb)
                {
                    if (item == ri.NewItem && (type == RequestBuilder.ElementType.Item || type == RequestBuilder.ElementType.Unknown))
                    {
                        gb = rb.GetGroupFor(ri.OldItem, type);
                        return true;
                    }
                    gb = default;
                    return false;
                }
            }
        }

        private static RequestBuilder.RequestBuilderUpdateHandler CreateRemover(RemoverInfo info)
        {
            void RemoveItem(RequestBuilder rb)
            {
                if (!info.IsActive()) return;
                if (!rb.gs.SkipSettings.GetFieldByName(info.SkipSetting)) return;

                rb.ReplaceItem(info.OldItem, info.NewItem);
                rb.ReplaceItem(PlaceholderItem.Prefix + info.OldItem, PlaceholderItem.Prefix + info.NewItem);
                rb.StartItems.Replace(info.OldItem, info.NewItem);

                if (!string.IsNullOrEmpty(info.VanillaLocation))
                {
                    rb.RemoveFromVanilla(info.VanillaLocation, info.OldItem);
                }
            }

            return RemoveItem;
        }
    }
}

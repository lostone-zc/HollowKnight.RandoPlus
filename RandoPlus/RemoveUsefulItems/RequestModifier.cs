﻿using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace RandoPlus.RemoveUsefulItems
{
    public static class RequestModifier
    {
        public static readonly (string oldItem, string newItem, Func<bool> isActive)[] settings = new (string oldItem, string newItem, Func<bool> isActive)[]
        {
            (ItemNames.Lumafly_Lantern, Consts.NoLantern,() => RandoPlus.GS.没有灯笼),
            (ItemNames.Ismas_Tear, Consts.NoTear, () => RandoPlus.GS.没有酸泪),
            (ItemNames.Swim, Consts.NoSwim, () => RandoPlus.GS.没有游泳)
        };

        public static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(-499f, SetupRefs);

            foreach ((string oldItem, string newItem, Func<bool> isActive) in settings)
            {
                RequestBuilder.OnUpdate.Subscribe(50f, CreateRemover(oldItem, newItem, isActive));
            }
        }

        private static void SetupRefs(RequestBuilder rb)
        {
            if (!RandoPlus.GS.Any) return;

            foreach ((string oldItem, string newItem, Func<bool> isActive) in settings)
            {
                rb.EditItemRequest(newItem, info =>
                {
                    info.getItemDef = () => new ItemDef()
                    {
                        Name = newItem,
                        Pool = Consts.RemoveUsefulItems,
                        MajorItem = false,
                        PriceCap = 500
                    };
                });

                rb.OnGetGroupFor.Subscribe(-999f, MatchGroup);

                bool MatchGroup(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb)
                {
                    if (item == newItem && (type == RequestBuilder.ElementType.Item || type == RequestBuilder.ElementType.Unknown))
                    {
                        gb = rb.GetGroupFor(oldItem, type);
                        return true;
                    }
                    gb = default;
                    return false;
                }
            }
        }

        private static RequestBuilder.RequestBuilderUpdateHandler CreateRemover(string oldItem, string newItem, Func<bool> isRandomized)
        {
            void RemoveItem(RequestBuilder rb)
            {
                if (!isRandomized()) return;

                rb.ReplaceItem(oldItem, newItem);
                rb.ReplaceItem(PlaceholderItem.Prefix + oldItem, PlaceholderItem.Prefix + newItem);
                rb.StartItems.Replace(oldItem, newItem);

                List<VanillaRequest> vanilla = rb.Vanilla.EnumerateWithMultiplicity().Where(x => x.Item == oldItem).ToList();
                foreach (VanillaRequest req in vanilla)
                {
                    rb.Vanilla.RemoveAll(req);
                    rb.Vanilla.Add(new(newItem, req.Location));
                }
            }

            return RemoveItem;
        }
    }
}

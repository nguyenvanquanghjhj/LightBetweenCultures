using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightBetweenCultures
{
    public sealed class JourneyShopEntry
    {
        public readonly string id,itemId,name,description,completedFlag,completedQuest;
        public readonly int price;
        public bool Permission=>itemId==null;
        public JourneyShopEntry(string id,string item,string name,int price,string text,string flag=null,string quest=null)
        {this.id=id;itemId=item;this.name=name;this.price=price;description=text;completedFlag=flag;completedQuest=quest;}
    }

    // A fixed allowlist. No city, discovery, story or shop-level visibility gates.
    // Uses the existing coins/items/quest states and reward ledger, save v1/v2.
    public sealed class JourneyShop
    {
        public static readonly IReadOnlyList<JourneyShopEntry> Catalog=Array.AsReadOnly(new[]{
            new JourneyShopEntry("chili","hoi_an_fresh_chili","Ớt tươi Hội An",1,"Gia vị giao lưu và phần chuẩn bị đêm hội. Mua từng phần."),
            new JourneyShopEntry("shallot","hoi_an_purple_shallot","Hành tím Hội An",1,"Gia vị cho đêm hội hoặc giao lưu với Huế."),
            new JourneyShopEntry("meat","meat_portion","Thức ăn làm mồi",1,"Dụ cua ở H12, cáo ở G6 hoặc giao 3 phần tại D10 Đà Lạt. Không hồi HP; không dùng trong trận."),
            new JourneyShopEntry("tea","healing_tea","Trà hồi phục",2,"Trong trận: bấm Uống trà để hồi tối đa 14 HP. Tốn 1 ly và 1 lượt; đối thủ đáp trả. Không hồi năng lượng."),
            new JourneyShopEntry("herb","royal_herb","Thảo dược bờ bắc",2,"Mang tới bà Nguyệt tại Huế để tự xác nhận giao thuốc.",quest:"UQ02"),
            new JourneyShopEntry("flower","night_flower","Hoa đêm",2,"Mang tới người làm vườn Đà Lạt để tự xác nhận giao hoa.",quest:"DQ04"),
            new JourneyShopEntry("glasses","lan_glasses","Kính của Lan",3,"Mang về cho Lan tại Đà Lạt. Không tự giải trụ sương.","da_lat.glasses_returned"),
            new JourneyShopEntry("strawberry","da_lat_strawberry","Dâu Đà Lạt",2,"Một phần dâu cho yêu cầu giao lưu với Jeonju.",quest:"DQ05")
        });
        readonly GameBootstrap game;
        bool busy;
        public JourneyShop(GameBootstrap owner){game=owner;}
        public JourneyShopEntry Find(string id){foreach(var e in Catalog)if(e.id==id)return e;return null;}
        public bool Owned(JourneyShopEntry e)
        {
            if(e==null)return false;
            if(!string.IsNullOrEmpty(e.completedQuest)&&game.Quests.IsCompleted(e.completedQuest))return true;
            if(!string.IsNullOrEmpty(e.completedFlag)&&game.World.Has(e.completedFlag))return true;
            if(e.Permission)return game.Save.rewardGrantIds.Contains("shop:v1:unlock:HQ04");
            return game.Content.Item(e.itemId)?.stackLimit==1&&
                (game.Inventory.Has(e.itemId)||game.Save.rewardGrantIds.Contains("shop:v1:unique:"+e.itemId));
        }
        public string Unavailable(JourneyShopEntry e)
        {
            if(e==null)return "Mặt hàng không được bán.";
            if(SaveSystem.RecoveryBlocked)return "Cần khôi phục bản lưu trước khi mua.";
            if(Owned(e))return "Đã có / đã hoàn tất";
            if(!e.Permission&&game.Inventory.Count(e.itemId)>=(game.Content.Item(e.itemId)?.stackLimit??0))return "Túi đã đủ số lượng";
            return game.Save.coins<e.price?"Thiếu "+(e.price-game.Save.coins)+" xu":"";
        }
        public bool TryBuy(string id,string requestId,out string message)
        {
            message="";var entry=Find(id);
            if(busy){message="Giao dịch đang được lưu.";return false;}
            if(!Guid.TryParse(requestId,out _)){message="Giao dịch không hợp lệ.";return false;}
            var receipt="shop:v1:purchase:"+requestId;
            if(game.Save.rewardGrantIds.Contains(receipt)){message="Giao dịch này đã được ghi nhận.";return false;}
            message=Unavailable(entry);if(message.Length>0)return false;
            busy=true;var before=JsonUtility.ToJson(game.Save);var attemptedSave=false;
            try
            {
                if(!game.Inventory.TryConsume("coin",entry.price))return false;
                if(entry.Permission)
                {
                    if(!game.Quests.CompletePaidFerryFare())throw new InvalidOperationException("Phí đò không còn phù hợp cấu hình hiện tại.");
                    game.Save.rewardGrantIds.Add("shop:v1:unlock:HQ04");
                }
                else
                {
                    game.Inventory.Add(entry.itemId);
                    if(game.Content.Item(entry.itemId).stackLimit==1)game.Save.rewardGrantIds.Add("shop:v1:unique:"+entry.itemId);
                }
                game.Save.rewardGrantIds.Add(receipt);
                attemptedSave=true;
                if(!game.Persist())throw new InvalidOperationException("Không lưu được giao dịch. Xu và vật phẩm đã được giữ nguyên; hãy thử lại sau.");
                message=entry.Permission?"Đã trả phí đò Hội An · quyền đò đã mở.":"Đã mua "+entry.name+" ×1 · −"+entry.price+" xu";
                return true;
            }
            catch(Exception e)
            {
                JsonUtility.FromJsonOverwrite(before,game.Save);game.Discovery.Refresh();
                // A failed File.Replace/Move may leave a valid temporary file.
                // Rewrite that pending payload with the pre-purchase state too,
                // so a later recovery cannot silently charge a rejected sale.
                if(attemptedSave)SaveSystem.Write(game.Save);
                message=e.Message;return false;
            }
            finally{busy=false;}
        }
    }
}

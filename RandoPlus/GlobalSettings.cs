namespace RandoPlus
{
    public class GlobalSettings
    {
        public bool 自定义用总开关;

        public bool 蘑菇先生随机;
        public bool 复制蘑菇孢子;
        
        public bool 没有酸泪;
        public bool 没有灯笼;
        public bool 没有游泳;

        public bool 随机到七张图内;
        public bool 一白点多物品;

        [Newtonsoft.Json.JsonIgnore]
        public bool Any => 自定义用总开关
            || 蘑菇先生随机
            || 没有游泳
            || 没有酸泪
            || 没有灯笼
            || 随机到七张图内;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinbusCompanyLocalizeCustomer.JsonClass
{
    #region 负责人格名称读取
    public class IdtList
    {
        public List<IdtInfo> dataList = new List<IdtInfo>();
    }
    public class IdtInfo
    {
        public int id { get; set; } //数字id，用于技能识别
        public string title { get; set; } //人格名
        public string name { get; set; } //罪人名
        public string nameWithTitle { get; set; } //也是罪人名
        public string desc { get; set; } //内部描述，一般是xxx的第几人格
    }
    #endregion
    #region 人格技能数据读取
    public class SkillInfo
    {
        //从原版的levelList加载名称
        public SkillInfo(string id,dynamic levelList)
        {
            Skill_id = id;
            Skill_name = levelList[0].name;
        }
        // parameterless ctor for creating custom entries
        public SkillInfo() { }
        public string Skill_id { get; set; }
        public string Skill_name { get; set; }
        public string File_name { get; set; }//记录所属文件,用于后续导出为json
    }
    #endregion
    #region 自定义汉化数据储存
    public class LCLCFileJson
    {
        public List<IdtInfo>IdtNameList = new List<IdtInfo>();
        public List<SkillInfo>SkillInfoList = new List<SkillInfo>();
    }
    #endregion
}

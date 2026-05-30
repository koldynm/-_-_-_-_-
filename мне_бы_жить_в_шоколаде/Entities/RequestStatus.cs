using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace мне_бы_жить_в_шоколаде.Entities
{
    public class EnumValue
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public EnumValue(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static List<EnumValue> RequestStatuses = [
            new EnumValue("Новый", "new"),
            new EnumValue("В процессе", "in_progress"),
            new EnumValue("Завершен", "closed")
        ];

        public static List<EnumValue> RequestPriorities = [
            new EnumValue("Низкий", "low"),
            new EnumValue("Средний", "medium"),
            new EnumValue("Высокий", "high")
        ];


    }
}

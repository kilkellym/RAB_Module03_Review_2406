using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RAB_Module03_Review_2406
{
    public class  FurnitureType
    {
        public string Name { get; set; }
		public string FamilyName { get; set; }
		public string TypeName { get; set; }

		public FurnitureType(string _name, string _familyName, string _typeName)
		{
			Name = _name;
			FamilyName = _familyName;
			TypeName = _typeName;
		}
		public FurnitureType()
		{

		}
	}
	public class FurnitureSet
	{
		public string Set { get; set; }
		public string RoomType { get; set; }
		public string[] Furniture { get; private set; }

		public FurnitureSet(string _set, string _roomType, string _furniture)
		{
			Set = _set;
			RoomType = _roomType;
			Furniture = _furniture.Split(',');
		}

		public int GetFurnitureCount()
		{
			return Furniture.Count();
		}

	}
}

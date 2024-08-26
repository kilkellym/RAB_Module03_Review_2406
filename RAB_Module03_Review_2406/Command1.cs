using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;

namespace RAB_Module03_Review_2406
{
	[Transaction(TransactionMode.Manual)]
	public class Command1 : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			// Revit application and document variables
			UIApplication uiapp = commandData.Application;
			UIDocument uidoc = uiapp.ActiveUIDocument;
			Document doc = uidoc.Document;

			// 1. get furniture data
			List<string[]> furnitureTypeList = GetFurnitureTypes();
			List<string[]> furnitureSetList = GetFurnitureSets();
			furnitureSetList.RemoveAt(0); // remove header row
			furnitureTypeList.RemoveAt(0); // remove header row

			// 2. populate furniture data classes
			List<FurnitureType> furnitureTypes = new List<FurnitureType>();

			foreach(string[] furnitureType in furnitureTypeList)
			{
				FurnitureType newType = new FurnitureType(furnitureType[0], furnitureType[1], furnitureType[2]);
				furnitureTypes.Add(newType);furnitureTypes.Add(new FurnitureType(furnitureType[0], furnitureType[1], furnitureType[2]));
			}

			List<FurnitureSet> furnitureSets = new List<FurnitureSet>();

			foreach (string[] furnSets in furnitureSetList)
			{
				FurnitureSet newSet = new FurnitureSet(furnSets[0], furnSets[1], furnSets[2]);
				furnitureSets.Add(newSet);
			}

			// 3. get rooms from model
			FilteredElementCollector collector = new FilteredElementCollector(doc);
			collector.OfCategory(BuiltInCategory.OST_Rooms);

			// 4. loop through rooms
			using (Transaction t = new Transaction(doc))
			{
				t.Start("Move in furniture");
				int counter = 0;

				foreach(SpatialElement curRoom in collector)
				{
					// 5. get room data
					LocationPoint roomPoint = curRoom.Location as LocationPoint;
					XYZ insPoint = roomPoint.Point;

					// 6. get room set
					string furnSet = curRoom.LookupParameter("Furniture Set").AsString();

					// 7. find matching set
					foreach(FurnitureSet curSet in furnitureSets)
					{
						if(curSet.Set == furnSet)
						{
							foreach(string furnItem in curSet.Furniture)
							{
								FamilySymbol curSymbol = GetFurnitureByName(doc, furnitureTypes, furnItem);

								if(curSymbol != null)
								{
									FamilyInstance curFI = doc.Create.NewFamilyInstance(insPoint, curSymbol,
										StructuralType.NonStructural);

									counter++;
								}
							}
						}

						SetParameterValue(curRoom, "Furniture Count", curSet.GetFurnitureCount());
					}
				}

				t.Commit();

				TaskDialog.Show("Result", "Furniture added to " + counter + " rooms.");

			}

			return Result.Succeeded;
		}

		private void SetParameterValue(SpatialElement curRoom, string paramName, int value)
		{
			Parameter curParam = curRoom.LookupParameter(paramName);

			if(curParam != null) 
			{
				curParam.Set(value);
			}
		}
		private void SetParameterValue(SpatialElement curRoom, string paramName, string value)
		{
			Parameter curParam = curRoom.LookupParameter(paramName);

			if (curParam != null)
			{
				curParam.Set(value);
			}
		}

		private FamilySymbol GetFurnitureByName(Document doc, List<FurnitureType> furnitureTypes, string tmpfurnItem)
		{
			string furnItem = tmpfurnItem.Trim();

			foreach (FurnitureType curType in furnitureTypes)
			{
				if (curType.Name == furnItem)
				{
					FamilySymbol curFS = GetFamilySymbolByName(doc, curType.FamilyName, curType.TypeName);

					if (curFS != null)
					{
						if (curFS.IsActive == false)
						{
							curFS.Activate();
						}
					}

					return curFS;
				}
			}

			return null;
		}

		private FamilySymbol GetFamilySymbolByName(Document doc, string familyName, string typeName)
		{
			FilteredElementCollector collector = new FilteredElementCollector(doc);
			collector.OfClass(typeof(FamilySymbol));

			foreach (FamilySymbol curFS in collector)
			{
				if (curFS.FamilyName == familyName && curFS.Name == typeName)
				{
					return curFS;
				}
			}

			return null;
		}

		private List<string[]> GetFurnitureTypes()
		{
			List<string[]> returnList = new List<string[]>();
			returnList.Add(new string[] { "Furniture Name", "Revit Family Name", "Revit Family Type" });
			returnList.Add(new string[] { "desk", "Desk", "60in x 30in" });
			returnList.Add(new string[] { "task chair", "Chair-Task", "Chair-Task" });
			returnList.Add(new string[] { "side chair", "Chair-Breuer", "Chair-Breuer" });
			returnList.Add(new string[] { "bookcase", "Shelving", "96in x 12in x 84in" });
			returnList.Add(new string[] { "loveseat", "Sofa", "54in" });
			returnList.Add(new string[] { "teacher desk", "Table-Rectangular", "48in x 30in" });
			returnList.Add(new string[] { "student desk", "Desk", "60in x 30in Student" });
			returnList.Add(new string[] { "computer desk", "Table-Rectangular", "48in x 30in" });
			returnList.Add(new string[] { "lab desk", "Table-Rectangular", "72in x 30in" });
			returnList.Add(new string[] { "lounge chair", "Chair-Corbu", "Chair-Corbu" });
			returnList.Add(new string[] { "coffee table", "Table-Coffee", "30in x 60in x 18in" });
			returnList.Add(new string[] { "sofa", "Sofa-Corbu", "Sofa-Corbu" });
			returnList.Add(new string[] { "dining table", "Table-Dining", "30in x 84in x 22in" });
			returnList.Add(new string[] { "dining chair", "Chair-Breuer", "Chair-Breuer" });
			returnList.Add(new string[] { "stool", "Chair-Task", "Chair-Task" });

			return returnList;
		}

		private List<string[]> GetFurnitureSets()
		{
			List<string[]> returnList = new List<string[]>();
			returnList.Add(new string[] { "Furniture Set", "Room Type", "Included Furniture" });
			returnList.Add(new string[] { "A", "Office", "desk, task chair, side chair, bookcase" });
			returnList.Add(new string[] { "A2", "Office", "desk, task chair, side chair, bookcase, loveseat" });
			returnList.Add(new string[] { "B", "Classroom - Large", "teacher desk, task chair, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk" });
			returnList.Add(new string[] { "B2", "Classroom - Medium", "teacher desk, task chair, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk" });
			returnList.Add(new string[] { "C", "Computer Lab", "computer desk, computer desk, computer desk, computer desk, computer desk, computer desk, task chair, task chair, task chair, task chair, task chair, task chair" });
			returnList.Add(new string[] { "D", "Lab", "teacher desk, task chair, lab desk, lab desk, lab desk, lab desk, lab desk, lab desk, lab desk, stool, stool, stool, stool, stool, stool, stool" });
			returnList.Add(new string[] { "E", "Student Lounge", "lounge chair, lounge chair, lounge chair, sofa, coffee table, bookcase" });
			returnList.Add(new string[] { "F", "Teacher's Lounge", "lounge chair, lounge chair, sofa, coffee table, dining table, dining chair, dining chair, dining chair, dining chair, bookcase" });
			returnList.Add(new string[] { "G", "Waiting Room", "lounge chair, lounge chair, sofa, coffee table" });

			return returnList;
		}
		internal static PushButtonData GetButtonData()
		{
			// use this method to define the properties for this command in the Revit ribbon
			string buttonInternalName = "btnCommand1";
			string buttonTitle = "Button 1";

			Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
				buttonInternalName,
				buttonTitle,
				MethodBase.GetCurrentMethod().DeclaringType?.FullName,
				Properties.Resources.Blue_32,
				Properties.Resources.Blue_16,
				"This is a tooltip for Button 1");

			return myButtonData.Data;
		}
	}

}

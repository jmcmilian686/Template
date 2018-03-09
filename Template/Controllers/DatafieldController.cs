using FileGenerator.Domain.Abstract;
using FileGenerator.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FileGenerator.Models;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Template.Controllers
{
    public class DatafieldController : Controller
    {
        private IFieldsRepository fieldsRepo;
        private IDataFieldRepository datafieldRepo;
       

        public DatafieldController(IFieldsRepository fieldRepository, IDataFieldRepository datafieldRepository)
        {
            this.fieldsRepo = fieldRepository;
            this.datafieldRepo = datafieldRepository;

        }

        // GET: Datafield
        public ActionResult Index(int? id = 0)
        {
            List<Field> fields = fieldsRepo.Fields.ToList();

            if ((fields.Count > 0)&&(id== 0))
            {
                ViewBag.FirstId = fields[1].ID;
            }
            else if( id > 0) {

                ViewBag.FirstId = id;
            }


            return View(fields);
        }


        public ActionResult Details(int id) {
            List<DataField> datafields;
            if (id == 0)
            {
                datafields = new List<DataField>();
            }
            else {
                datafields = datafieldRepo.DataFields.Where(p => p.FieldID == id).ToList();
            }

            return PartialView("_DetTable", datafields);
        }


        [HttpGet]
        public ActionResult Create(int? id) {

            List<Field> fields = fieldsRepo.Fields.ToList();

            List<DataField> datafields = new List<DataField>();

            ViewBag.FieldName = fields;

            return View(datafields);
        }


        [HttpPost]
        public ActionResult Create(TableViewModel data)
        {
            try {
                if (data.DataValue.Count() > 1)
                { // more than one element, linked elements

                    string textName = "", textValue = "";

                    foreach (var e in data.DataValue)
                    {
                        textName += e.Name;
                        textValue += e.Val;
                    }

                    MD5 md5Hasher = MD5.Create();
                    var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(textName));
                    var Link_P = BitConverter.ToInt32(hashed, 0);

                    hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(textValue));
                    var Link_S = BitConverter.ToInt32(hashed, 0);

                    foreach (var j in data.DataValue)
                    {
                        DataField dataf = new DataField
                        {
                            FieldID = fieldsRepo.Fields.Where(p => p.Field_Name == j.Name).FirstOrDefault().ID,
                            Data = j.Val,
                            Link_P = Link_P,
                            Link_S = Link_S
                        };
                        datafieldRepo.SaveDataField(dataf);
                    }

                }
                else {// only one element
                    string name = data.DataValue[0].Name;
                    string value = data.DataValue[0].Val;
                    DataField dataf = new DataField
                    {
                        
                        FieldID = fieldsRepo.Fields.Where(p => p.Field_Name == name).FirstOrDefault().ID,
                        Data = value,

                    };
                    datafieldRepo.SaveDataField(dataf);
                }
                return Json("ok");

            }
            catch(Exception e) {

                return Json(e.Data.Values);
            }
        }

            

            
       


        public ActionResult Table(TableViewModel data = null) {

       

        TableViewModel mod = new TableViewModel();
           List<Elements> dict = new List<Elements>();
            if (data.DataValue!= null && data.DataValue.Count() > 0) {
                dict = data.DataValue;

            }
            if (data.ID > 0)
            {
                Field field = fieldsRepo.Fields.Where(p => p.ID == data.ID).FirstOrDefault();
                dict.Add(new Elements { Name = field.Field_Name, Val = "" });

            }
            else {

            }
            
            
            mod.ID = 0;
            mod.DataValue =dict;

            return PartialView("_Table",mod);


        }

        public ActionResult LinkedList(int id) {

            DataField elem = datafieldRepo.DataFields.Where(p => p.ID == id).FirstOrDefault();

            List<DataField> linkedData = datafieldRepo.DataFields.Where(p => p.Link_P == elem.Link_P).ToList();
            List<Elements> dict = new List<Elements>();

            foreach (var m in linkedData) {

                dict.Add(new Elements { Name = m.Field.Field_Name, Val = m.Data });

            }
            TableViewModel mod = new TableViewModel();
            mod.ID = 0;
            mod.DataValue = dict;

            return PartialView("_Table", mod);


        }

        public ActionResult Delete(int id) {

            try {

                if (id > 0)
                {

                    DataField datafield = datafieldRepo.DataFields.Where(p => p.ID == id).FirstOrDefault();

                    if (datafield.Link_P != null)
                    {

                        List<DataField> lstData = datafieldRepo.DataFields.Where(p => p.Link_P == datafield.Link_P).ToList();
                        List<int> lstDataId = new List<int>();
                        foreach (var m in lstData)
                        {

                            lstDataId.Add(m.ID);

                        }

                        foreach (int i in lstDataId)
                        {

                            datafieldRepo.DeleteDataField(id);

                        }


                    }
                    else
                    {
                        
                        datafieldRepo.DeleteDataField(id);

                    }

                    return Json("ok");

                }

                return Json("Index 0");

            }
            catch (Exception e) {

                return Json(e.Message);

            }

            
            

        }

    }
}
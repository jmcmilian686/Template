using FileGenerator.Domain.Abstract;
using FileGenerator.Domain.Entities;
using FileGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Template.Controllers
{
    public class GenfileController : Controller
    {
        private IFieldsRepository fieldsRepo;
        private IDataFieldRepository datafieldRepo;
        private ILFileRepository docRepo;
        private IStructRepository structRepo;
        private IStructFieldRepository structFieldRepo;


        public GenfileController(IFieldsRepository fieldRepository, IDataFieldRepository datafieldRepository, ILFileRepository docRepository, IStructRepository structRepository, IStructFieldRepository structFieldRepository)
        {
            this.fieldsRepo = fieldRepository;
            this.datafieldRepo = datafieldRepository;
            this.docRepo = docRepository;
            this.structRepo = structRepository;
            this.structFieldRepo = structFieldRepository;

        }
        // GET: Genfile
        public ActionResult Index()
        {
            ViewBag.Organization = datafieldRepo.DataFields.Where(p => p.Field.Field_Name == "ORGANIZATION_ID").ToList();

            ViewBag.DCenter = datafieldRepo.DataFields.Where(p => p.Field.Field_Name == "DISTRIBUTION_CENTER").ToList();

            ViewBag.Bussiness = datafieldRepo.DataFields.Where(p => p.Field.Field_Name == "BUSINESS_UNIT").ToList();

            ViewBag.OrderT = datafieldRepo.DataFields.Where(p => p.Field.Field_Name == "ORDER_TYPE").ToList();

            ViewBag.Document = docRepo.LFiles.ToList();



            return View();
        }

        public ActionResult Create(FormCreateViewModel model) {

            if (ModelState.IsValid) {

                LFile doc = docRepo.LFiles.Where(p => p.LFile_ID == model.Document).FirstOrDefault();
                List<Struct> docStruct = structRepo.Structs.Where(p => p.LFile_ID == doc.LFile_ID).ToList();

                FileViewModel file = new FileViewModel();

                file.Rows = new List<FRowViewModel>();

                if (docStruct != null) {

                    foreach (var elem in docStruct) {

                        List<StructField> sfields = structFieldRepo.StructFields.Where(p => p.StructID == elem.ID).OrderBy(p=>p.Field_Order).ToList();

                        FRowViewModel row = new FRowViewModel();
                        row.DataShow = new Dictionary<string, string>();

                        foreach (var m in sfields) {

                            row.DataShow.Add(m.Field.Field_Name, "");

                        }

                        row.Level = 0;

                        // checking the inbound model for values

                        List<DataField> paramet = new List<DataField>();


                        DataField organization = datafieldRepo.DataFields.Where(p => p.ID == model.Organization).FirstOrDefault();
                        paramet.Add(organization);

                        DataField dcenter = datafieldRepo.DataFields.Where(p => p.ID == model.Dist_Center).FirstOrDefault();
                        paramet.Add(dcenter);

                        DataField bunit = datafieldRepo.DataFields.Where(p => p.ID == model.Bussiness_Unit).FirstOrDefault();
                        paramet.Add(bunit);

                        DataField ordType = datafieldRepo.DataFields.Where(p => p.ID == model.Order_Type).FirstOrDefault();
                        paramet.Add(ordType);

                        int numSts = docStruct.Count();

                        if (elem.Order_In_Doc == 1)
                        {
                            row.DataShow["RECORD TYPE"] = "H";

                        }
                        else if (elem.Order_In_Doc == numSts)
                        {

                            row.DataShow["RECORD TYPE"] = "C";

                        }
                        else {
                            row.DataShow["RECORD TYPE"] = "D";

                        }

                        
                        foreach (var m in paramet) {


                            if (row.DataShow.ContainsKey(m.Field.Field_Name))
                            {
                                if (row.DataShow[m.Field.Field_Name] == "") {

                                    if (m.Link_P != null)
                                    {// if there is a link with other elements

                                        List<DataField> linkElem = datafieldRepo.DataFields.Where(p => p.Link_P == m.Link_P && p.Link_S == m.Link_S).ToList();

                                        if (linkElem != null)
                                        {

                                            foreach (var k in linkElem)
                                            {// let's find out if the linked elements are in the document

                                                if (row.DataShow.ContainsKey(k.Field.Field_Name))
                                                {

                                                    row.DataShow[k.Field.Field_Name] = k.Data;
                                                }

                                            }
                                        }

                                    }
                                    row.DataShow[m.Field.Field_Name] = m.Data;
                                }

                               
                               
                            }

                        }

                        // Now we search in db for the rest of values 

                        Dictionary<string, string> aux = new Dictionary<string, string>();

                        foreach (var dict in row.DataShow) {

                            aux.Add(dict.Key, "");


                            if (dict.Value == "") {

                                var rand = new Random();
                                IEnumerable<DataField> insValue = datafieldRepo.DataFields.Where(p => p.Field.Field_Name == dict.Key).AsEnumerable();
                                int amount = insValue.Count();


                                if (amount > 0)
                                {
                                    DataField valor = insValue.Skip(rand.Next(0, amount)).First();
                                    aux[dict.Key] = valor.Data;

                                }
                                else {

                                    aux[dict.Key] = dict.Key.Substring(0, 4) + elem.Order_In_Doc;
                                }


                            }
                            else {
                                aux[dict.Key] = dict.Value;
                            }

                        }

                        row.DataShow = aux;

                        file.Rows.Add(row);

                    }


                }


                return View(viewName: "FileData", model: file);

            }

            return View();
        }
    }
}
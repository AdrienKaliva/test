using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanierTache.Interface;
using PanierTache.Interface.PortailDessin;
using PanierTache.Models;
using PanierTache.OutilModel;
using PanierTache.OutilRechercheModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PanierTache.Controllers
{

    public class DocumentController : BaseCrudController
    {
        private IDocumentService _iDocumentService;
        private IColumnVisibleService _iColumnVisibleService;
        private IGenericService _iGenericService;
        private IGridUtilisateurTemplateColumnService _iGridUtilisateurTemplateColumnService;
        private IInitListSelect _iInitListSelect;
        private IParameterGridColonneService _iParameterGridColonneService;
        private IPortailUrlService _iPortailUrlService;
        private IProfilRechercheCatalogueService _iProfilRechercheCatalogueService;
        private IRechercheCatalogueService _iRechercheCatalogueService;
        private ITemplateGridService _iTemplateGridService;
        private IUtilisateurService _iUtilisateurService;
        private IHubModuleTable _iHubModuleTable;
        private IHubModule _iHubModule;
        private IProfil_RechercheTemplateService _iProfil_RechercheTemplateService;
        private IUtilisateurRechercheTemplateService _iUtilisateurRechercheTemplateService;
        private IParameterUrlNextGenService<ReportingDocumentModel> _iParameterUrlNextGenService;
        private IBaseService<ReportingDocument, ReportingDocumentModel> _iBaseService;
        public ILog _iLog;

        public DocumentController(IFunctionalitieService pIFunctionalitieService, IInitListSelect pIInitListSelect, IParameterGridColonneService pIParameterGridColonneService,
             IProfilFuncService pIProfilFuncService, IRechercheCatalogueService pIRechercheCatalogueService, ITemplateGridService pITemplateGridService,
            IProfil_RechercheTemplateService pIProfil_RechercheTemplateService, IUtilisateurRechercheTemplateService pIUtilisateurRechercheTemplateService,
            IHubModuleTable pIHubModuleTable, IDocumentService pDocumentService, IColumnVisibleService pIColumnVisibleService, IGridUtilisateurTemplateColumnService pIGridUtilisateurTemplateColumnService,
            IGenericService pIGenericService, IUtilisateurService pIUtilisateurService, IHubModule pIHubModule, IProfilRechercheCatalogueService pIProfilRechercheCatalogueService,
            IParameterUrlNextGenService<ReportingDocumentModel> pIParameterUrlNextGenService, IPortailUrlService pIPortailUrlService, IGenericService pGenericService, IBaseService<ReportingDocument, ReportingDocumentModel> pIBaseService,
            ILog pILog)
            : base(pIFunctionalitieService, pIInitListSelect, pIParameterGridColonneService, pIProfilFuncService, pIRechercheCatalogueService, pITemplateGridService, pIProfil_RechercheTemplateService,
                  pIUtilisateurRechercheTemplateService, pGenericService, pIHubModuleTable, pIColumnVisibleService, pIProfilRechercheCatalogueService, pIUtilisateurService)
        {

            this._iInitListSelect = pIInitListSelect;
            this._iParameterGridColonneService = pIParameterGridColonneService;
            this._iRechercheCatalogueService = pIRechercheCatalogueService;
            this._iTemplateGridService = pITemplateGridService;
            this._iProfil_RechercheTemplateService = pIProfil_RechercheTemplateService;
            this._iUtilisateurRechercheTemplateService = pIUtilisateurRechercheTemplateService;
            this._iDocumentService = pDocumentService;
            this._iColumnVisibleService = pIColumnVisibleService;
            this._iGenericService = pIGenericService;
            this._iGridUtilisateurTemplateColumnService = pIGridUtilisateurTemplateColumnService;
            this._iUtilisateurService = pIUtilisateurService;
            this._iHubModuleTable = pIHubModuleTable;
            this._iHubModule = pIHubModule;
            this._iPortailUrlService = pIPortailUrlService;
            this._iProfilRechercheCatalogueService = pIProfilRechercheCatalogueService;
            this._iParameterUrlNextGenService = pIParameterUrlNextGenService;
            this._iBaseService = pIBaseService;
            _iLog = pILog;
        }

        [RedirectingAction]
        [NoCache]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index(int? idTemplate, string SearchString, string onglet, string arg, string find, string cancel, int? idRecherche, int? user, int? profilId)
        {
            // Initialiser les variable 
            #region 
            string _searchname = string.Empty;
            List<string> selectedIDS = null;
            var recherModels = new List<RechercheModel>();
            int? idModule = _iHubModuleTable.GetModuleIdByTableName("ReportingDocument");
            RechercheCatalogueModel defautS = null;
            ViewBag.searchnamet = true;
            string searchfiltre = null;
            SetOngletActive("Document");
            Session["_ongletActive"] = "Document";
            TempData["ongletActive"] = "Document";
            TempData["idGrid"] = "gridDocument";
            #endregion

            // Check session 
            #region
            if (user != null && profilId != null)
            {
                SetUserId(user.ToString());
                SetProfilId(profilId);
            }
            int profil = GetProfilId();
            string userId = GetUserId();
            if (profil == 0 || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            #endregion

            //Get idTemplate
            #region
            if (idTemplate == null && Session["IdTemplateDocument"] != null)
            {
                idTemplate = GetidTemplateDocument();
            }
            if (idTemplate == null)
            {
                idTemplate = getIdTemplateDefaut(userId, "ReportingDocument");
                SetidTemplateDocument(idTemplate);
            }
            #endregion

            //Get ColModel & ColName
            #region
            var colM = GetColModel(idTemplate);
            Session["ColModelDocument"] = colM;
            var colModel = JsonConvert.SerializeObject(colM, Newtonsoft.Json.Formatting.Indented);
            var colName = new JavaScriptSerializer().Serialize(getColName(colM));
            #endregion

            //Recherche
            #region 
            recherModels = GetSearchModule1Model();
            idRecherche = (idRecherche == null && Session["IdRecherche"] != null) ? GetIdRechercheModule1Model() : idRecherche;
            if (cancel != "Cancel" && arg != "--" && recherModels == null && idRecherche == null)
            {
                SetRechercheDefautEtTemplateDefaut(ref defautS, userId, idModule, ref recherModels, ref idRecherche, ref idTemplate);
            }
            if (idRecherche != null && idRecherche != 0)
            {
                RechercheCatalogueModel recherche = defautS != null ? defautS : _iRechercheCatalogueService.GetByIDWithBtDelete((int)idRecherche, Convert.ToInt32(userId));
                if (recherche.Public != null && recherche.Public == true) ViewBag.RecherchePublique = true;
                ViewBag.ButtonDelete = recherche.ButtonDelete;
                ViewBag.ButtonSave = recherche.ButtonSave;
                ViewBag.Default = (recherche.DefaultSearch != null && recherche.DefaultSearch == 1) ? true : false;
                searchfiltre = recherche.RechercheName;
                ViewBag.DisableCheckboxPublique = recherche.DisableCheckboxPublique;
                ViewBag.Favoris = recherche.Favoris != null && recherche.Favoris == true;
            }
            int idFiltre = idRecherche != null ? (int)idRecherche : 0;
            selectedIDS = (Session["SelectedDocumentIds"] != null) ? Session["SelectedDocumentIds"] as List<string> : selectedIDS;
            if (selectedIDS != null)
            {
                ViewBag.colSelected = selectedIDS;
            }
            else ViewBag.colSelected = "null";
            if (recherModels == null) recherModels = new List<RechercheModel>();
            else
            {
                InitViewBagForRechercheAvance(recherModels);
            }
            if (recherModels.Count > 0) ViewBag.DeleteCButton = true;
            else ViewBag.DeleteCButton = null;
            #endregion


            var userTemplate = _iTemplateGridService.GetByID(idTemplate);
            string templateName = userTemplate != null ? ("Modèle : " + userTemplate.TemplateName) : "Template de base";
            string filtreName = searchfiltre != null ? ("Filtre : " + searchfiltre) : "Aucun filtre";
            SetViewBagGrid("gridDocument", "pagerDocument", "oui", colModel, colName, templateName + " - " + filtreName, "rowObject.ID", "ID", "ReportingDocument", "Document", SearchString, idTemplate, Convert.ToInt32(userId));
            ViewData["critereRechercheListNoElem"] = new SelectList(new Dictionary<int, string>() { }, "Key", "Value");
            var refTables = _iHubModuleTable.GetRefTableListByModule(idModule);
            ViewData["critereRechercheList"] = new SelectList(_iInitListSelect.GetCritereRecherche(profil, _iColumnVisibleService, refTables), "Key", "Value");
            ViewData["rechercheCatalogueList"] = new SelectList(_iInitListSelect.GetRechercheCatalogue(Convert.ToInt32(userId), profil, _iProfilRechercheCatalogueService, _iRechercheCatalogueService, idModule), "Key", "Value", idFiltre);
            ViewData["rechercheFavoris"] = new SelectList(_iInitListSelect.GetFavoris(Convert.ToInt32(userId), idModule, _iRechercheCatalogueService), "Key", "Value", idFiltre);
            ViewBag.searchfiltre = searchfiltre != null ? searchfiltre : "--";
            InitFunctionalities(profil, userId);
            ViewBag.Filter = Session["FilterDocument"] != null ? Session["FilterDocument"].ToString() : "NoFilter";
            SetViewBagToShowSordIcon(idTemplate);

            return View(recherModels);
        }

        public override List<object> GetList(JsonModel jsonModel, int page, int rows)
        {
            DataRetourModel _cmdData = new DataRetourModel();
            SearchRuleModel searchRuleModel = GetSearchRule(jsonModel, "ReportingDocument");

            //The null element is related to break the assosiation between Devis and document
            var list = _iBaseService.GetListGrid(searchRuleModel.JsonModel, searchRuleModel.ProjetSearchRules, searchRuleModel.TacheSearchRules, searchRuleModel.CommercialSearchRules, searchRuleModel.DocumentSearchRules,
                searchRuleModel.SuiviDocumentSearchRules, null, null , searchRuleModel.SelectedProjetIds, searchRuleModel.SelectedTacheIds,
                searchRuleModel.SelectedCommercialIds, searchRuleModel.SelectedDocumentIds, searchRuleModel.SelectedSuiviDocumentIds, searchRuleModel.SelectedDevisIds, searchRuleModel.SelectedDemandesIds, searchRuleModel.Op,
            searchRuleModel.JsonProfilModel, searchRuleModel.ProjetProfilSearchRules, searchRuleModel.TacheProfilSearchRules, searchRuleModel.CommercialProfilSearchRules, searchRuleModel.DocumentProfilSearchRules,
            searchRuleModel.SuiviDocumentProfilSearchRules, null, null , _iGenericService, _iParameterGridColonneService, "dbo.ReportingDocuments", ref _cmdData);

            if (list != null)
            {
                var specificColumns = new List<string>() { "UrlNextGen", "UrlNextGenActeur", "UrlNextGenDocument", "UrlNextGenGabarit", "UrlNextGenProjet"};
                var colModel = Session["ColModelDocument"] as List<JqGridModel>;
                var check = colModel.Where(p => specificColumns.Contains(p.name)).Any();
                if (check)
                {
                    int count = searchRuleModel.Count;
                    if (count * searchRuleModel.Page > list.Count)
                    {
                        count = list.Count - count * (searchRuleModel.Page - 1);
                    }

                    //Get the urls to add to construction
                    List<Parameter_Url_NextGen> urlList = _iParameterUrlNextGenService.GetAllTypeUrl();
                    List<Portail_Url> portailList = _iPortailUrlService.GetAllPortail();

                    for (int i = searchRuleModel.Index; i < searchRuleModel.Index + count; i++)
                    {
                        if (CheckColumnExist("UrlNextGen", colModel))
                        {
                            list[i] = _iParameterUrlNextGenService.SetLienNextgen(list[i], _iPortailUrlService, _iParameterUrlNextGenService, "Document", urlList, portailList);
                        }
                        if (CheckColumnExist("UrlDocument", colModel))
                        {
                            list[i] = _iParameterUrlNextGenService.SetLienDocument(list[i], _iPortailUrlService, _iParameterUrlNextGenService, "Document", urlList, portailList);
                        }
                        if (CheckColumnExist("UrlNextGenActeur", colModel))
                        {
                            list[i] = _iParameterUrlNextGenService.SetOneLienNextgen(list[i], "ACTEUR", "UrlNextGenActeur", "Acteur", "user", _iPortailUrlService, _iParameterUrlNextGenService, null, urlList, portailList);
                        }
                        if (CheckColumnExist("UrlNextGenDocument", colModel))
                        {
                            list[i] = _iParameterUrlNextGenService.SetOneLienNextgen(list[i], "CONTRAC_PROJET", "UrlNextGenDocument", "Documents", "book", _iPortailUrlService, _iParameterUrlNextGenService, "Document", urlList, portailList);
                        }
                        if (CheckColumnExist("UrlNextGenGabarit", colModel))
                        {
                            list[i] = _iParameterUrlNextGenService.SetOneLienNextgen(list[i], "GABARIT", "UrlNextGenGabarit", "Gabarit", "cog", _iPortailUrlService, _iParameterUrlNextGenService, null, urlList, portailList);
                        }
                        if (CheckColumnExist("UrlNextGenProjet", colModel))
                        {
                            list[i] = _iParameterUrlNextGenService.SetOneLienNextgen(list[i], "ACCUEIL_PROJET", "UrlNextGenProjet", "Projet", "home", _iPortailUrlService, _iParameterUrlNextGenService, null, urlList, portailList);
                        }
                    }
                }
            }
            else
            {
                list = new List<ReportingDocumentModel>();
            }
            List<string> idsRow = list.Select(p => p.ID.ToString()).ToList();
            Session["GridDocumentList"] = list;
            Session["idsRowDocument"] = idsRow;

            //Set the true length
            SetGridListLength(_cmdData.length);
            //Set the query to get the data
            SetGridProjetCmd(_cmdData.cmd);

            return list.Cast<object>().ToList();
        }
        public ActionResult DownloadDocument (string id)
        {
            var lien = _iDocumentService.GetLienDocument(id, "CONTRAC_PROJET", "UrlNextGenDocument", "Documents", "book", _iPortailUrlService);
            return Content(lien);
        }
        public async Task<ActionResult> CheckDocumentArchived(string id)
        {
            var result = await _iDocumentService.GetLienDocumentArchived(id, "CONTRAC_PROJET", _iPortailUrlService, _iLog, GetUserId());
            if (result != null)
            {
                Session["DocumentArchived"] = result.Item1;
                Session["DocumentArchivedName"] = result.Item2;
                return Content("/Document/DownloadDocumentArchived");
            }
            else
            {
                Session["DocumentArchived"] = null;
                return Content("KO");
            }
        }
        public ActionResult DownloadDocumentArchived()
        {
            var stream = Session["DocumentArchived"] as Stream;
            if (stream != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    string mimeType = "application/zip";
                    Session["DocumentArchived"] = null;
                    return new FileContentResult(ms.ToArray(), mimeType)
                    {
                        FileDownloadName = Session["DocumentArchivedName"] as string
                    };
                }
            }
            else
            {
                Session["DocumentArchived"] = null;
                return Content("KO");
            }
        }

        public override List<JqGridModel> GetColModel(int? idTemplate)
        {
            var colModels = new List<JqGridModel>();
            colModels = _iParameterGridColonneService.getColModel("ReportingDocument", idTemplate, GetProfilId(), _iColumnVisibleService, _iGridUtilisateurTemplateColumnService, _iParameterGridColonneService);
            var colM = colModels.Where(p => p.index == "ID").FirstOrDefault();
            var exitDateM = colModels.Where(p => p.index == "DateModification").FirstOrDefault();
            var exitDateStatutDoc = colModels.Where(p => p.index == "StatutDoc").FirstOrDefault();
            if (exitDateM == null)
            {
                JqGridModel date = new JqGridModel("DateModification", 1, null, false, "DateModification");
                date.index = "DateModification";
                date.formatoptions = new JRaw("{ srcformat: 'd/m/Y H:i:s', newformat: 'd/m/Y H:i:s' }");
                date.formatter = new JRaw(@"""date""");
                colModels.Add(date);
            }
            if (exitDateStatutDoc == null)
            {
                JqGridModel statut = new JqGridModel("StatutDoc", 1, null, false, "StatutDoc");
                statut.index = "StatutDoc";
                statut.editable = false;
                colModels.Add(statut);
            }
            if (colM != null)
            {
                colM.key = true;
            }
            else
            {
                JqGridModel id = new JqGridModel("ID", 1, _iParameterGridColonneService.getMaxSize(_iParameterGridColonneService.getGridColonnes("ReportingDocument", idTemplate, GetProfilId(), _iColumnVisibleService, _iGridUtilisateurTemplateColumnService, _iParameterGridColonneService)), false, "ID");
                id.key = true;
                id.editable = false;
                colModels.Add(id);
            }
            return colModels;
        }


        public JsonResult GetIdsRow()
        {

            object jsonErrorData = null;

            if (Session["idsRowDocument"] != null)
            {
                List<string> idsL = Session["idsRowDocument"] as List<string>;
                return Json(new { ids = idsL }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                jsonErrorData = new
                {
                    Error = "Error"
                };
                return Json(jsonErrorData, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        [RedirectingAction]
        public ActionResult SaveEditLine(ReportingDocumentModel model, string dateModification)
        {
            if (GetUserId() == null)
            {
                return Content("redirect");
            }
            try
            {
                Session["RowEditDocument"] = model;
                Boolean isStatutValid = _iDocumentService.CheckUpdateByStatutDoc(model.ID);
                {
                    if (isStatutValid == false)
                    {
                        return Content("NOEDIT");
                    }
                    else
                    {
                        Boolean isValid = _iDocumentService.CheckUpdateById(model.ID, dateModification);
                        if (isValid == true)
                        {
                            var checkData = CheckListDataForcolModels("ReportingDocument", (BaseModel)model);
                            if (checkData == false)
                            {
                                return Content("DataInCorrect");
                            }
                            return Content("OK");
                        }
                        else
                        {
                            return Content("KO");
                        }
                    }
                }

            }
            catch
            {
                return Content("Error");
            }
        }
        public ActionResult SaveEditLineAfterCheck(string status)
        {
            var colModels = Session["ColModelDocument"] as List<JqGridModel>;
            var rowEditModel = Session["RowEditDocument"] != null ? Session["RowEditDocument"] as ReportingDocumentModel : new ReportingDocumentModel();
            int? _idTemplate = Session["IdTemplateDocument"] != null ? GetidTemplateDocument() : null;
            if (status == "OK")
            {
                string login = _iUtilisateurService.GetByID(Convert.ToInt32(GetUserId())).UserLogin;
                _iDocumentService.Update(rowEditModel, colModels, login);
            }
            return RedirectToAction("Index", "Document", new { idTemplate = _idTemplate });
        }

        [HttpPost]
        [RedirectingAction]
        [NoCache]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult EditCell(ReportingDocumentModel model, string dateModification, string celname, string value)
        {
            if (GetUserId() == null)
            {
                return Content("redirect");
            }
            try
            {
                var check = CheckListData(model.ID, null, celname, value, "ReportingDocument");
                if (check == false)
                {
                    return Content("DataInCorrect");
                }
                Boolean isValid = _iDocumentService.CheckUpdateById(model.ID, dateModification, model.DateModification);
                if (isValid == true)
                {
                    _iDocumentService.UpdateUnChamp(model, celname, GetUserId(), _iUtilisateurService);
                    return Content("OK");
                }
                else
                {
                    Session["RowEditDocument"] = model;
                    Session["CelNameEdit"] = celname;
                    return Content("KO");
                }
            }
            catch
            {
                return Content("Error");
            }
        }
        [RedirectingAction]
        [NoCache]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CheckUpdateByStatutDoc(string RowId)

        {
            Boolean isStatutValid = _iDocumentService.CheckUpdateByStatutDoc(new Guid(RowId));
            {
                if (isStatutValid == false)
                {
                    return Content("NOEDIT");
                }
            }
            return Content("EDIT");
        }

        [RedirectingAction]
        [NoCache]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult SaveEditCellAfterCheck(string status)
        {
            int? _idTemplate = null;
            if (status == "OK")
            {
                var _rowEditModel = (Session["RowEditDocument"] != null) ? Session["RowEditDocument"] as ReportingDocumentModel : new ReportingDocumentModel();
                var celname = Session["CelNameEdit"] as string;
                _iDocumentService.UpdateUnChamp(_rowEditModel, celname, GetUserId(), _iUtilisateurService);
            }
            if (Session["IdTemplateDocument"] != null) _idTemplate = GetidTemplateDocument();
            return RedirectToAction("Index", "Document", new { idTemplate = _idTemplate });

        }
    }
}
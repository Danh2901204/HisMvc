const drugService = require('./drug.service');
const { renderWithLayout } = require('../../utils/render');

const listDrugs = async (req, res) => {
  const drugs = await drugService.getAllDrugs();
  renderWithLayout(res, 'pages/admin/drugs/list', {
    title: 'Quản lý thuốc',
    drugs
  });
};

const createDrugForm = (req, res) => {
  renderWithLayout(res, 'pages/admin/drugs/create', {
    title: 'Thêm thuốc'
  });
};

const createDrug = async (req, res) => {
  await drugService.createDrug(req.body);
  res.redirect('/admin/drugs');
};

const editDrugForm = async (req, res) => {
  const drug = await drugService.getDrugById(req.params.id);
  renderWithLayout(res, 'pages/admin/drugs/edit', {
    title: 'Cập nhật thuốc',
    drug
  });
};

const updateDrug = async (req, res) => {
  await drugService.updateDrug(req.params.id, req.body);
  res.redirect('/admin/drugs');
};

const deleteDrug = async (req, res) => {
  await drugService.deleteDrug(req.params.id);
  res.redirect('/admin/drugs');
};

module.exports = {
  listDrugs,
  createDrugForm,
  createDrug,
  editDrugForm,
  updateDrug,
  deleteDrug,
};
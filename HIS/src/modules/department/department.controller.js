const departmentService = require('./department.service');

function renderWithLayout(req, res, view, data = {}) {
  res.render('layouts/main', {
    body: `pages/admin/departments/${view}`,
    ...data
  });
}

const listDepartments = async (req, res) => {
  const departments = await departmentService.getAllDepartments();
  renderWithLayout(req, res, 'list', { 
    title: 'Quản lý phòng ban',
    departments
  });
};

const createDepartmentForm = (req, res) => {
  renderWithLayout(req, res, 'create', {
    title: 'Thêm phòng ban'
  });
};

const createDepartment = async (req, res) => {
  await departmentService.createDepartment(req.body);
  res.redirect('/admin/departments');
};

const editDepartmentForm = async (req, res) => {
  const department = await departmentService.getDepartmentById(req.params.id);
  renderWithLayout(req, res, 'edit', {
    title: 'Cập nhật phòng ban',
    department
  });
};

const updateDepartment = async (req, res) => {
  await departmentService.updateDepartment(req.params.id, req.body);
  res.redirect('/admin/departments');
};

const deleteDepartment = async (req, res) => {
  await departmentService.deleteDepartment(req.params.id);
  res.redirect('/admin/departments');
};

module.exports = {
  listDepartments,
  createDepartmentForm,
  createDepartment,
  editDepartmentForm,
  updateDepartment,
  deleteDepartment,
};
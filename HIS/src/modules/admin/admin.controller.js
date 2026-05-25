const adminService = require('./admin.service');
const departmentService = require('../department/department.service');
const { USER_ROLES } = require('../../shared/enums');

const renderUserManagement = async (req, res) => {
  try {
    const users = await adminService.getUsers();
    const departments = await departmentService.getAllDepartments();
    res.render('layouts/main', {
      body: 'pages/admin/users',
      title: 'User Management',
      users,
      departments,
      roles: Object.values(USER_ROLES),
      error: null,
      success: req.query.success,
    });
  } catch (error) {
    res.status(500).send(error.message);
  }
};

const createUser = async (req, res) => {
    try {
      await adminService.createUser(req.body);
      res.redirect('/admin/users?success=User created successfully');
    } catch (error) {
      const users = await adminService.getUsers();
      const departments = await departmentService.getAllDepartments();
      res.render('layouts/main', {
        body: 'pages/admin/users',
        title: 'User Management',
        users,
        departments,
        roles: Object.values(USER_ROLES),
        error: error.message,
        success: null,
      });
    }
  };

const deleteUser = async (req, res) => {
  try {
    await adminService.deleteUserById(req.params.id);
    res.redirect('/admin/users?success=User deleted successfully');
  } catch (error) {
    const users = await adminService.getUsers();
    const departments = await departmentService.getAllDepartments();
    res.render('layouts/main', {
      body: 'pages/admin/users',
      title: 'User Management',
      users,
      departments,
      roles: Object.values(USER_ROLES),
      error: 'Error deleting user: ' + error.message,
      success: null,
    });
  }
};

const renderEditUserForm = async (req, res) => {
  try {
    const user = await adminService.getUserById(req.params.id);
    const departments = await departmentService.getAllDepartments();
    if (!user) {
      return res.status(404).send('User not found');
    }
    res.render('layouts/main', {
      body: 'pages/admin/edit-user',
      title: 'Edit User',
      user,
      departments,
      roles: Object.values(USER_ROLES),
      error: null,
      success: null,
    });
  } catch (error) {
    res.status(500).send(error.message);
  }
};

const updateUser = async (req, res) => {
  try {
    await adminService.updateUser(req.params.id, req.body);
    res.redirect('/admin/users?success=User updated successfully');
  } catch (error) {
    const user = await adminService.getUserById(req.params.id);
    const departments = await departmentService.getAllDepartments();
    res.render('layouts/main', {
      body: 'pages/admin/edit-user',
      title: 'Edit User',
      user,
      departments,
      roles: Object.values(USER_ROLES),
      error: 'Error updating user: ' + error.message,
      success: null,
    });
  }
};

module.exports = {
  renderUserManagement,
  createUser,
  deleteUser,
  renderEditUserForm,
  updateUser,
};
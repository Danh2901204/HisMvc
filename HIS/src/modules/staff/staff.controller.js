const staffService = require('./staff.service');
const adminService = require('../admin/admin.service'); // Use adminService to create user
const { renderWithLayout } = require('../../utils/render');

const listStaff = async (req, res, next) => {
  try {
    const staff = await staffService.getAllStaff();
    renderWithLayout(res, 'pages/admin/staff/list', {
      title: 'Staff List',
      staff: staff,
    });
  } catch (error) {
    next(error);
  }
};

const showCreateForm = async (req, res, next) => {
  try {
    renderWithLayout(res, 'pages/admin/staff/create', {
      title: 'Create Staff',
    });
  } catch (error) {
    next(error);
  }
};

const createStaffMember = async (req, res, next) => {
  try {
    // 1. Create User Account
    const { username, password, role, ...staffData } = req.body;
    const newUser = await adminService.createUser({ username, password, role });

    // 2. Create Staff Profile and link to User Account
    staffData.userAccount = newUser._id;
    const newStaff = await staffService.createStaff(staffData);

    // 3. (Optional) Update User with link to Staff profile
    // This creates a two-way relationship if needed
    // await adminService.updateUser(newUser._id, { staffProfile: newStaff._id });

    res.redirect('/admin/staff');
  } catch (error) {
    // If something goes wrong, we might want to delete the created user
    // to avoid orphaned user accounts. This is a simple error handling.
    // A more robust solution might use database transactions.
    if (error.code === 11000) { // Handle duplicate username error
        error.message = `Username '${req.body.username}' đã tồn tại.`;
    }
    next(error);
  }
};

const showEditForm = async (req, res, next) => {
  try {
    const staffMember = await staffService.getStaffById(req.params.id);
    if (!staffMember) {
      return res.status(404).send('Staff not found');
    }
    renderWithLayout(res, 'pages/admin/staff/edit', {
      title: 'Edit Staff',
      staffMember: staffMember,
    });
  } catch (error) {
    next(error);
  }
};

const updateStaffMember = async (req, res, next) => {
  try {
    const staffId = req.params.id;
    const staffData = req.body;
    const { username, password, role } = req.body;

    // If username and password are provided, create a user account
    if (username && password) {
      const newUser = await adminService.createUser({ username, password, role });
      staffData.userAccount = newUser._id;
    }

    await staffService.updateStaff(staffId, staffData);
    res.redirect('/admin/staff');
  } catch (error) {
    next(error);
  }
};

const deleteStaffMember = async (req, res, next) => {
  try {
    await staffService.deleteStaff(req.params.id);
    res.redirect('/admin/staff');
  } catch (error) {
    next(error);
  }
};

const showCreateAccountForm = async (req, res, next) => {
  try {
    const staffMember = await staffService.getStaffById(req.params.id);
    if (!staffMember) {
      return res.status(404).send('Staff not found');
    }
    renderWithLayout(res, 'pages/admin/staff/create-account', {
      title: 'Create Account',
      staffMember: staffMember,
    });
  } catch (error) {
    next(error);
  }
};

const createAccountForStaff = async (req, res, next) => {
  try {
    const { username, password, role } = req.body;
    const staffId = req.params.id;

    // 1. Create User Account
    const newUser = await adminService.createUser({ username, password, role });

    // 2. Link User Account to Staff Profile
    await staffService.updateStaff(staffId, { userAccount: newUser._id });

    res.redirect('/admin/staff');
  } catch (error) {
    if (error.code === 11000) { // Handle duplicate username error
        error.message = `Username '${req.body.username}' đã tồn tại.`;
    }
    next(error);
  }
};

module.exports = {
  listStaff,
  showCreateForm,
  createStaffMember,
  showEditForm,
  updateStaffMember,
  deleteStaffMember,
  showCreateAccountForm,
  createAccountForStaff,
};
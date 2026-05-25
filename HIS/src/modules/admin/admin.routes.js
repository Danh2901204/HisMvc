const express = require('express');
const router = express.Router();
const adminController = require('./admin.controller');
const { protect } = require('../../middlewares/auth.middleware');
const requireRole = require('../../middlewares/requireRole');
const { PERMISSIONS } = require('../../config/permissions');
const invoiceController = require('../invoice/invoice.controller');

// Import other routes
const staffRoutes = require('../staff/staff.route');
const bedRoutes = require('../bed/bed.routes');
const departmentRoutes = require('../department/department.routes');
const drugRoutes = require('../drug/drug.routes');
const roomRoutes = require('../room/room.routes');

// All routes in this file are protected and require admin role
router.use(protect, requireRole(PERMISSIONS.MANAGE_USERS));

// Redirect /admin to /admin/users
router.get('/', (req, res) => {
  res.redirect('/admin/users');
});

// Mount other routes
router.use('/staff', staffRoutes);
router.use('/beds', bedRoutes);
router.use('/departments', departmentRoutes);
router.use('/drugs', drugRoutes);
router.use('/rooms', roomRoutes);

// @route   GET /admin/invoices
// @desc    Display invoice list
router.get('/invoices', invoiceController.listInvoices);

// @route   GET /admin/invoices/edit/:id
// @desc    Show form to edit an invoice
router.get('/invoices/edit/:id', invoiceController.editInvoiceForm);

// @route   POST /admin/invoices/edit/:id
// @desc    Update an invoice
router.post('/invoices/edit/:id', invoiceController.updateInvoice);

// @route   POST /admin/invoices/delete/:id
// @desc    Delete an invoice
router.post('/invoices/delete/:id', invoiceController.deleteInvoice);

// @route   GET /admin/users
// @desc    Display user management page
router.get('/users', adminController.renderUserManagement);

// @route   POST /admin/users
// @desc    Create a new user
router.post('/users', adminController.createUser);

// @route   POST /admin/users/delete/:id
// @desc    Delete a user
router.post('/users/delete/:id', adminController.deleteUser);

// @route   GET /admin/users/edit/:id
// @desc    Show form to edit a user
router.get('/users/edit/:id', adminController.renderEditUserForm);

// @route   POST /admin/users/edit/:id
// @desc    Update a user
router.post('/users/edit/:id', adminController.updateUser);

module.exports = router;
const express = require('express');
const router = express.Router();
const invoiceController = require('./invoice.controller');
const { isAuthenticated, hasRole } = require('../../middlewares/auth.middleware');

// Các route cho quản lý hóa đơn, yêu cầu đăng nhập và có vai trò 'reception'
router.get('/', isAuthenticated, hasRole(['reception']), invoiceController.listInvoices);
router.get('/create', isAuthenticated, hasRole(['reception']), invoiceController.createInvoiceForm);
router.post('/create', isAuthenticated, hasRole(['reception']), invoiceController.createInvoice);
router.get('/edit/:id', isAuthenticated, hasRole(['reception']), invoiceController.editInvoiceForm);
router.post('/edit/:id', isAuthenticated, hasRole(['reception']), invoiceController.updateInvoice);
router.post('/delete/:id', isAuthenticated, hasRole(['reception']), invoiceController.deleteInvoice);

module.exports = router;
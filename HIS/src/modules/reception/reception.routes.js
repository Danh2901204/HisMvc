const express = require("express");
const router = express.Router();
const c = require("./reception.controller");
const { protect } = require("../../middlewares/auth.middleware");
const requireRole = require("../../middlewares/requireRole");
const { PERMISSIONS } = require("../../config/permissions");
const invoiceController = require("../invoice/invoice.controller");

// Patients
router.get("/patients", protect, c.listPatients);
router.get("/patients/new", protect, requireRole(PERMISSIONS.ACCESS_RECEPTION), c.newPatientForm);
router.post("/patients", protect, requireRole(PERMISSIONS.ACCESS_RECEPTION), c.createPatient);

// Visits (đợt khám)
router.get("/visits/new", protect, requireRole(PERMISSIONS.ACCESS_RECEPTION), c.newVisitForm);
router.post("/visits", protect, requireRole(PERMISSIONS.ACCESS_RECEPTION), c.createVisit);

// Invoices
router.get('/invoices', protect, requireRole(PERMISSIONS.ACCESS_RECEPTION), invoiceController.listInvoices);
router.get('/invoices/create', protect, requireRole(PERMISSIONS.ACCESS_RECEPTION), invoiceController.createInvoiceForm);
router.post('/invoices/create', protect, requireRole(PERMISSIONS.ACCESS_RECEPTION), invoiceController.createInvoice);

module.exports = router;
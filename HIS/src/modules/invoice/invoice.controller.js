const Invoice = require('./invoice.model');
const Patient = require('../reception/models/patient.model');
const Visit = require('../reception/models/visit.model');
const { USER_ROLES } = require('../../shared/enums');

// Hiển thị danh sách hóa đơn
exports.listInvoices = async (req, res) => {
    try {
        const invoices = await Invoice.find();
        const layout = req.user.role === USER_ROLES.ADMIN ? 'layouts/main' : 'layouts/reception';
        res.render(layout, {
            body: 'pages/reception/invoice/list',
            invoices,
            user: req.user,
            title: 'Quản lý hóa đơn'
        });
    } catch (error) {
        res.status(500).send(error.toString());
    }
};

// Hiển thị form tạo hóa đơn
exports.createInvoiceForm = async (req, res) => {
    try {
        const patients = await Patient.find();
        const visits = await Visit.find();
        res.render('pages/reception/invoice/create', {
            layout: 'layouts/reception',
            patients,
            visits
        });
    } catch (error) {
        res.status(500).send(error.toString());
    }
};

// Tạo hóa đơn mới
exports.createInvoice = async (req, res) => {
    try {
        const { visitCode, patientCode, ...rest } = req.body;
        const visit = await Visit.findOne({ visitCode });
        const patient = await Patient.findOne({ patientCode });

        if (!visit || !patient) {
            return res.status(404).send('Visit or Patient not found');
        }

        const newInvoice = new Invoice({
            ...rest,
            visit: visit._id,
            patient: patient._id,
        });

        await newInvoice.save();
        res.redirect('/reception/invoices');
    } catch (error) {
        res.status(500).send(error.toString());
    }
};

// Hiển thị form chỉnh sửa hóa đơn
exports.editInvoiceForm = async (req, res) => {
    try {
        const invoice = await Invoice.findById(req.params.id);
        if (!invoice) {
            return res.status(404).send('Invoice not found');
        }
        const layout = req.user.role === USER_ROLES.ADMIN ? 'layouts/main' : 'layouts/reception';
        res.render('pages/reception/invoice/edit', {
            invoice,
            layout
        });
    } catch (error) {
        res.status(500).send(error.toString());
    }
};

// Cập nhật hóa đơn
exports.updateInvoice = async (req, res) => {
    try {
        await Invoice.findByIdAndUpdate(req.params.id, req.body);
        res.redirect('/reception/invoices');
    } catch (error) {
        res.status(500).send(error.toString());
    }
};

// Xóa hóa đơn
exports.deleteInvoice = async (req, res) => {
    try {
        await Invoice.findByIdAndDelete(req.params.id);
        res.redirect('/reception/invoices');
    } catch (error) {
        res.status(500).send(error.toString());
    }
};
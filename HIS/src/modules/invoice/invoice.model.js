const mongoose = require('mongoose');

const invoiceSchema = new mongoose.Schema({
  invoiceCode: {
    type: String,
    required: true,
    unique: true,
  },
  visit: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'Visit',
    required: true,
  },
  patient: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'Patient',
    required: true,
  },
  items: [
    {
      name: String,
      quantity: Number,
      price: Number,
    },
  ],
  total: {
    type: Number,
    required: true,
  },
  status: {
    type: String,
    enum: ['Chưa thanh toán', 'Đã thanh toán'],
    default: 'Chưa thanh toán',
  },
}, { timestamps: true });

const Invoice = mongoose.model('Invoice', invoiceSchema);

module.exports = Invoice;
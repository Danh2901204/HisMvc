const mongoose = require('mongoose');

const labOrderSchema = new mongoose.Schema({
  patientId: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'Patient',
    required: true,
  },
  visitId: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'Visit',
    required: true,
  },
  labTestId: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'LabTest',
    required: true,
  },
  status: {
    type: String,
    enum: ['pending', 'completed'],
    default: 'pending',
  },
  result: {
    type: String,
  },
  date: {
    type: Date,
    default: Date.now,
  },
});

const LabOrder = mongoose.model('LabOrder', labOrderSchema);

module.exports = LabOrder;
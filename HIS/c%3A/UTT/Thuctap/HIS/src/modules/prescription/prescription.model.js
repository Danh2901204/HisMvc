const mongoose = require('mongoose');

const prescriptionSchema = new mongoose.Schema({
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
  drugId: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'Drug',
    required: true,
  },
  dosage: {
    type: String,
    required: true,
  },
  date: {
    type: Date,
    default: Date.now,
  },
});

const Prescription = mongoose.model('Prescription', prescriptionSchema);

module.exports = Prescription;
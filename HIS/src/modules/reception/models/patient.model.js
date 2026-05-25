const mongoose = require('mongoose');
const Schema = mongoose.Schema;

const patientSchema = new Schema({
    patientCode: { type: String, required: true, unique: true },
    fullName: { type: String, required: true },
    dateOfBirth: { type: Date, required: true },
    gender: { type: String, enum: ['Male', 'Female', 'Other'], required: true },
    address: { type: String },
    phone: { type: String },
    // Thêm các trường khác nếu cần
}, { timestamps: true });

const Patient = mongoose.model('Patient', patientSchema);

module.exports = Patient;
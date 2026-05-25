const mongoose = require('mongoose');

const departmentSchema = new mongoose.Schema({
  deptName: {
    type: String,
    required: true,
    trim: true,
    unique: true,
  },
  description: {
    type: String,
    trim: true,
  },
}, {
  timestamps: true,
});

const Department = mongoose.model('Department', departmentSchema, 'departments');

module.exports = Department;
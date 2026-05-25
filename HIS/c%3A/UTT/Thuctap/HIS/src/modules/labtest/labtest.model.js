const mongoose = require('mongoose');

const labTestSchema = new mongoose.Schema({
  name: {
    type: String,
    required: true,
    trim: true,
  },
  description: {
    type: String,
    trim: true,
  },
});

const LabTest = mongoose.model('LabTest', labTestSchema);

module.exports = LabTest;
const mongoose = require('mongoose');

const drugSchema = new mongoose.Schema({
  name: {
    type: String,
    required: true,
    trim: true,
  },
  description: {
    type: String,
    trim: true,
  },
  price: {
    type: Number,
    required: true,
  },
  quantityInStock: {
    type: Number,
    required: true,
    default: 0,
  },
}, {
  timestamps: true,
});

const Drug = mongoose.model('Drug', drugSchema);

module.exports = Drug;
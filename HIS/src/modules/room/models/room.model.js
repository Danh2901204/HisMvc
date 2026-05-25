const mongoose = require('mongoose');

const roomSchema = new mongoose.Schema({
  roomNumber: {
    type: String,
    required: true,
    trim: true,
  },
  department: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'Department',
    required: true,
  },
  roomType: {
    type: String,
    enum: ['Private', 'Semi-Private', 'Ward'],
    required: true,
  },
}, {
  timestamps: true,
});

const Room = mongoose.model('Room', roomSchema, 'Rooms');

module.exports = Room;
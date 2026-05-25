const mongoose = require('mongoose');
const Schema = mongoose.Schema;

const staffSchema = new Schema(
  {
    staffCode: {
      type: String,
      required: true,
      unique: true,
    },
    fullName: {
      type: String,
      required: true,
      trim: true,
    },
    email: {
      type: String,
      required: true,
      unique: true,
      trim: true,
      lowercase: true,
    },
    phone: {
      type: String,
      trim: true,
    },
    specialty: {
        type: String,
        trim: true,
    },
    staffType: {
        type: String,
    },
    deptCode: {
        type: String,
    },
    roomCode: {
        type: String,
    },
    userAccount: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'User',
    },
    isActive: {
      type: Boolean,
      default: true,
    },
  },
  {
    timestamps: true,
  }
);

// Chỉ định rõ ràng collection là 'staff' để tránh Mongoose tự động đổi tên
const Staff = mongoose.model('Staff', staffSchema, 'staff');

module.exports = Staff;
const mongoose = require('mongoose');
const Schema = mongoose.Schema;
const AutoIncrement = require('mongoose-sequence')(mongoose);

const visitSchema = new Schema({
    patient: { type: Schema.Types.ObjectId, ref: 'Patient', required: true },
    department: { type: String, required: true },
    room: { type: String },
    reason: { type: String, required: true },
    priority: { type: String, enum: ['low', 'medium', 'high'], default: 'low' },
    status: { type: String, default: 'waiting' },
    visitCode: { type: String, unique: true }
}, { timestamps: true });

visitSchema.plugin(AutoIncrement, { inc_field: 'visit_id', id: 'visit_id_counter' });

visitSchema.pre('save', async function (next) {
    if (this.isNew) {
        this.visitCode = `LK${String(this.visit_id).padStart(5, '0')}`;
    }
    next();
});

const Visit = mongoose.model('Visit', visitSchema);

module.exports = Visit;